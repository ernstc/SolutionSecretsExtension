using System.Collections.Generic;
using Microsoft.VisualStudio.Threading;
using Newtonsoft.Json;
using SolutionSecrets.Core;
using SolutionSecrets.Core.Repository;

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
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			var solutionFullName = SolutionSecrets2022Package._dte.Solution.FullName;

			SolutionFile solution = new SolutionFile(solutionFullName);

			var secretFiles = solution.GetProjectsSecretFiles();
			if (secretFiles.Count == 0)
			{
				await VS.StatusBar.ShowMessageAsync("No secrets found.");
				return;
			}

			var synchronizationSettings = solution.CustomSynchronizationSettings;

			// Select the repository for the curront solution
			IRepository repository = Context.Current.GetRepository(synchronizationSettings) ?? Context.Current.Repository;

			if (!await Context.Current.Cipher.IsReady() || !await repository.IsReady())
			{
				await VS.MessageBox.ShowAsync("You need to configure the solution secrets synchronization before using the Push command.",
					icon: Microsoft.VisualStudio.Shell.Interop.OLEMSGICON.OLEMSGICON_WARNING,
					buttons: Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_OK);

				var dialog = new ConfigDialog();
				dialog.ShowDialog();

				return;
			}

			await VS.StatusBar.ShowMessageAsync($"Pushing secrets for solution: {solution.Name} ...");

			var headerFile = new HeaderFile
			{
				visualStudioSolutionSecretsVersion = Versions.VersionString!,
				lastUpload = DateTime.UtcNow,
				solutionFile = solution.Name,
				solutionGuid = solution.Uid
			};

			var files = new List<(string fileName, string content)>
			{
				("secrets", JsonConvert.SerializeObject(headerFile, Formatting.None))
			};

			var secrets = new Dictionary<string, Dictionary<string, string>>();

			bool isEmpty = true;
			bool failed = false;
			foreach (var secretFile in secretFiles)
			{
				if (secretFile.Content != null)
				{
					isEmpty = false;
					bool isFileOk = true;

					if (repository.EncryptOnClient)
					{
						isFileOk = secretFile.Encrypt();
					}

					if (isFileOk)
					{
						if (!secrets.ContainsKey(secretFile.ContainerName))
						{
							secrets.Add(secretFile.ContainerName, new Dictionary<string, string>());
						}
						secrets[secretFile.ContainerName].Add(secretFile.Name, secretFile.Content);
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

			if (!isEmpty && !failed)
			{
				if (await repository.PushFilesAsync(solution, files))
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
