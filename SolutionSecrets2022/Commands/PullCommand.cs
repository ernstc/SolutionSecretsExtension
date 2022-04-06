using System.Collections.Generic;
using Microsoft.VisualStudio.Threading;
using Newtonsoft.Json;
using SolutionSecrets.Core;


namespace SolutionSecrets2022
{
    [Command(PackageIds.cmdidPullSecrets)]
    internal sealed class PullCommand : BaseCommand<PullCommand>
    {
        protected override void Execute(object sender, EventArgs e)
        {
			PullSecretsAsync().Forget();
		}


		private async Task PullSecretsAsync()
		{
			var _cipher = Services.Cipher;
			var _repository = Services.Repository;

			if (!await _cipher.IsReady() || !await _repository.IsReady())
			{
				await VS.MessageBox.ShowAsync("You need to configure the solution secrets synchronization before using the Pull command.",
					icon: Microsoft.VisualStudio.Shell.Interop.OLEMSGICON.OLEMSGICON_WARNING,
					buttons: Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_OK);

				var dialog = new ConfigDialog();
				dialog.ShowDialog();

				return;
			}

			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			var solutionFullName = SolutionSecrets2022Package._dte.Solution.FullName;

			SolutionFile solution = new SolutionFile(solutionFullName, _cipher);
			await VS.StatusBar.ShowMessageAsync($"Pulling secrets for solution: {solution.Name} ...");

			var configFiles = solution.GetProjectsSecretConfigFiles();
			if (configFiles.Count == 0)
			{
				await VS.StatusBar.ShowMessageAsync("No secrets found.");
				return;
			}

			_repository.SolutionName = solution.Name;

			var files = await _repository.PullFilesAsync();
			if (files.Count == 0)
			{
				await VS.StatusBar.ShowMessageAsync("No secrets found.");
				return;
			}

			// Validate header file
			HeaderFile header = null;
			foreach (var file in files)
			{
				if (file.name == "secrets" && file.content != null)
				{
					header = JsonConvert.DeserializeObject<HeaderFile>(file.content);
					break;
				}
			}

			if (header == null)
			{
				await VS.StatusBar.ShowMessageAsync("Error pulling secrets for the solution.");
				return;
			}

			Version headerVersion = new Version(header.visualStudioSolutionSecretsVersion);
			Version minVersion = new Version(Versions.MinimumFileFormatSupported);
			if (headerVersion.Major > minVersion.Major)
			{
				await VS.StatusBar.ShowMessageAsync("Secrets format not compatible.");
				return;
			}

			bool failed = false;
			foreach (var file in files)
			{
				if (file.name != "secrets")
				{
					if (file.content == null)
					{
						continue;
					}

					Dictionary<string, string> secretFiles = null;
					try
					{
						secretFiles = JsonConvert.DeserializeObject<Dictionary<string, string>>(file.content);
					}
					catch
					{
						await VS.StatusBar.ShowMessageAsync("Error pulling secrets for the solution.");
					}

					if (secretFiles == null)
					{
						failed = true;
						break;
					}

					foreach (var secret in secretFiles)
					{
						string configFileName = secret.Key;

						// This check is for compatibility with version 1.0.x
						if (configFileName == "content")
						{
							configFileName = "secrets.json";
						}

						foreach (var configFile in configFiles)
						{
							if (configFile.GroupName == file.name
								&& configFile.FileName == configFileName)
							{
								configFile.Content = secret.Value;
								if (configFile.Decrypt())
								{
									solution.SaveConfigFile(configFile);
								}
								else
								{
									failed = true;
								}
								break;
							}
						}
					}

					if (failed)
					{
						break;
					}
				}
			}

			if (!failed)
				await VS.StatusBar.ShowMessageAsync("Secrets pulled successfully.");
			else
				await VS.StatusBar.ShowMessageAsync("Secrets pull has failed!");
		}

    }
}
