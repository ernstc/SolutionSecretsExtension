using System.Collections.Generic;
using Microsoft.VisualStudio.Threading;
using Newtonsoft.Json;
using SolutionSecrets.Core;
using SolutionSecrets.Core.Repository;

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
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			var solutionFullName = SolutionSecrets2022Package._dte.Solution.FullName;

			SolutionFile solution = new SolutionFile(solutionFullName);

			ICollection<SecretFile> secretFiles = solution.GetProjectsSecretFiles();
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
				await VS.MessageBox.ShowAsync("You need to configure the solution secrets synchronization before using the Pull command.",
					icon: Microsoft.VisualStudio.Shell.Interop.OLEMSGICON.OLEMSGICON_WARNING,
					buttons: Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_OK);

				var dialog = new ConfigDialog();
				dialog.ShowDialog();

				return;
			}

			await VS.StatusBar.ShowMessageAsync($"Pulling secrets for solution: {solution.Name} ...");

			var repositoryFiles = await repository.PullFilesAsync(solution);
			if (repositoryFiles.Count == 0)
			{
				await VS.StatusBar.ShowMessageAsync("Failed, secrets not found.");
				return;
			}

			// Validate header file
			HeaderFile header = null;
			foreach (var file in repositoryFiles)
			{
				if (file.name == "secrets" && file.content != null)
				{
					try
					{
						header = JsonConvert.DeserializeObject<HeaderFile>(file.content);
					}
					catch
					{ }
					break;
				}
			}

			if (header == null)
			{
				await VS.StatusBar.ShowMessageAsync("Error pulling secrets for the solution.");
				return;
			}

			if (!header.IsVersionSupported())
			{
				await VS.StatusBar.ShowMessageAsync("Secrets format is not compatible.");
				return;
			}

			bool failed = false;
			foreach (var repositoryFile in repositoryFiles)
			{
				if (repositoryFile.name != "secrets")
				{
					if (repositoryFile.content == null)
					{
						continue;
					}

					Dictionary<string, string> remoteSecretFiles = null;

					try
					{
						remoteSecretFiles = JsonConvert.DeserializeObject<Dictionary<string, string>>(repositoryFile.content);
					}
					catch
					{
						await VS.StatusBar.ShowMessageAsync("Error pulling secrets for the solution.");
					}

					if (remoteSecretFiles == null)
					{
						failed = true;
						break;
					}

					foreach (var remoteSecretFile in remoteSecretFiles)
					{
						string secretFileName = remoteSecretFile.Key;

						// This check is for compatibility with version 1.0.x
						if (secretFileName == "content")
						{
							secretFileName = "secrets.json";
						}

						foreach (var localSecretFile in secretFiles)
						{
							if (localSecretFile.ContainerName == repositoryFile.name
								&& localSecretFile.Name == secretFileName)
							{
								localSecretFile.Content = remoteSecretFile.Value;

								bool isFileOk = true;
								if (repository.EncryptOnClient)
								{
									isFileOk = localSecretFile.Decrypt();
								}

								if (isFileOk)
								{
									solution.SaveSecretSettingsFile(localSecretFile);
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
