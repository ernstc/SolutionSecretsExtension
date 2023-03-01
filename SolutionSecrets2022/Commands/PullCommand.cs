using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Windows;
using Azure.Identity;
using Microsoft.VisualStudio.Threading;
using Newtonsoft.Json;
using SolutionSecrets.Core;
using SolutionSecrets.Core.Repository;


namespace SolutionSecrets2022
{
    [Command(PackageIds.cmdidPullSecrets)]
    internal sealed class PullCommand : BaseCommand<PullCommand>
    {

		protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
		{
			OleMenuCommandService commandService = await this.Package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
			PullSecretsAsync(commandService).Forget();
		}


		private async Task PullSecretsAsync(OleMenuCommandService commandService)
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

			await VS.StatusBar.ShowMessageAsync($"Pulling secrets from {repository.RepositoryTypeFullName} for the solution: {solution.Name} ...");

			if (repository is AzureKeyVaultRepository azureKvRepository)
			{
				if (!await azureKvRepository.IsReady())
				{
					try
					{
						await azureKvRepository.AuthorizeAsync();
					}
					catch (AuthenticationFailedException ex)
					{
						System.Windows.MessageBox.Show($"Azure authentication failed. Check your credential in\nTools -> Options -> Azure Service Authentication.", Vsix.Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);
						await VS.StatusBar.ShowMessageAsync(String.Empty);
						return;
					}
					catch (Exception)
					{
						await VS.StatusBar.ShowMessageAsync("Error pulling secrets for the solution.");
						return;
					}
				}

				if (!await azureKvRepository.IsReady())
				{
					await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
					System.Windows.MessageBox.Show($"Access denied to Azure Key Vault {azureKvRepository.RepositoryName}.", Vsix.Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);
					commandService.OpenOption<Options.AzureKeyVault.AzureKeyVaultOptionPage>();
					await VS.StatusBar.ShowMessageAsync("Error pulling secrets for the solution.");
					return;
				}
			}
			else if (!await Context.Current.Cipher.IsReady() || !await repository.IsReady())
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
				System.Windows.MessageBox.Show("You need to configure the solution secrets synchronization before using the Push command.", Vsix.Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);
				commandService.OpenOption<Options.GitHubGists.GitHubGistsOptionPage>();
				await VS.StatusBar.ShowMessageAsync("Error pulling secrets for the solution.");
				return;
			}


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
				await VS.StatusBar.ShowMessageAsync($"Secrets pulled successfully from {repository.RepositoryTypeFullName}.");
			else
				await VS.StatusBar.ShowMessageAsync("Secrets pull has failed!");
		}

    }
}
