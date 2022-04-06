using System.Collections.Generic;
using Microsoft.VisualStudio.Threading;
using Newtonsoft.Json;
using SolutionSecrets.Core;

namespace SolutionSecrets2022
{
    [Command(PackageIds.cmdidPushSecrets)]
    internal sealed class PushCommand : BaseCommand<PushCommand>
    {
		protected override void Execute(object sender, EventArgs e)
		{
			PushSecretsAsync().Forget();
		}


		private async Task PushSecretsAsync()
		{
			var _cipher = Services.Cipher;
			var _repository = Services.Repository;

			if (!await _cipher.IsReady() || !await _repository.IsReady())
			{
				await VS.MessageBox.ShowAsync("You need to configure the solution secrets synchronization before using the Push command.",
					icon: Microsoft.VisualStudio.Shell.Interop.OLEMSGICON.OLEMSGICON_WARNING,
					buttons: Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_OK);

				var dialog = new ConfigDialog();
				dialog.ShowDialog();

				return;
			}

			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			var solutionFullName = SolutionSecrets2022Package._dte.Solution.FullName;

			SolutionFile solution = new SolutionFile(solutionFullName, _cipher);
			await VS.StatusBar.ShowMessageAsync($"Pushing secrets for solution: {solution.Name} ...");

			var headerFile = new HeaderFile
			{
				visualStudioSolutionSecretsVersion = Vsix.Version,
				lastUpload = DateTime.UtcNow,
				solutionFile = solution.Name
			};

			List<(string fileName, string content)> files = new List<(string fileName, string content)>();
			files.Add(("secrets", JsonConvert.SerializeObject(headerFile, Formatting.None)));

			var configFiles = solution.GetProjectsSecretConfigFiles();
			if (configFiles.Count == 0)
			{
				return;
			}

			_repository.SolutionName = solution.Name;

			Dictionary<string, Dictionary<string, string>> secrets = new Dictionary<string, Dictionary<string, string>>();

			bool failed = false;
			foreach (var configFile in configFiles)
			{
				configFile.Load();
				if (configFile.Content != null)
				{
					if (configFile.Encrypt())
					{
						if (!secrets.ContainsKey(configFile.GroupName))
						{
							secrets.Add(configFile.GroupName, new Dictionary<string, string>());
						}
						secrets[configFile.GroupName].Add(configFile.FileName, configFile.Content);
					}
					else
					{
						failed = true;
						break;
					}
				}
			}

			foreach (var group in secrets)
			{
				string groupContent = JsonConvert.SerializeObject(group.Value);
				files.Add((group.Key, groupContent));
			}

			if (!failed)
			{
				if (await _repository.PushFilesAsync(files))
					await VS.StatusBar.ShowMessageAsync("Secrets pushed successfully.");
				else
					failed = true;
			}

			if (failed)
			{
				await VS.StatusBar.ShowMessageAsync("Secrets pushing has failed!");
			}
		}

	}
}
