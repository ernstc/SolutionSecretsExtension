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
    [Command(PackageIds.cmdidPushSecrets)]
    internal sealed class PushCommand : BaseCommand<PushCommand>
    {

		protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
		{
			OleMenuCommandService commandService = await this.Package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
			PushSecretsAsync(commandService).Forget();
		}


		private async Task PushSecretsAsync(OleMenuCommandService commandService)
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

			await VS.StatusBar.ShowMessageAsync($"Pushing secrets to {repository.RepositoryTypeFullName} for the solution: {solution.Name} ...");

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
						System.Windows.MessageBox.Show(ex.Message, Vsix.Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);
						await VS.StatusBar.ShowMessageAsync(String.Empty);
						return;
					}
					catch (Exception)
					{
						await VS.StatusBar.ShowMessageAsync("Error pushing secrets for the solution.");
						return;
					}
				}

				if (!await azureKvRepository.IsReady())
				{
					await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
					System.Windows.MessageBox.Show($"Access denied to Azure Key Vault {azureKvRepository.RepositoryName}.", Vsix.Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);
					commandService.OpenOption<Options.AzureKeyVault.AzureKeyVaultOptionPage>();
					await VS.StatusBar.ShowMessageAsync("Error pushing secrets for the solution.");
					return;
				}
			}
			else if (!await Context.Current.Cipher.IsReady() || !await repository.IsReady())
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
				System.Windows.MessageBox.Show("You need to configure the solution secrets synchronization before using the Push command.", Vsix.Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);
				commandService.OpenOption<Options.GitHubGists.GitHubGistsOptionPage>();
				await VS.StatusBar.ShowMessageAsync("Error pushing secrets for the solution.");
				return;
			}

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
					await VS.StatusBar.ShowMessageAsync($"Secrets pushed successfully to {repository.RepositoryTypeFullName}.");
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
