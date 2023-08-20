using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Windows;
using Azure.Identity;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;

using Newtonsoft.Json;

using SolutionSecrets.Core;
using SolutionSecrets.Core.Repository;
using Task = System.Threading.Tasks.Task;


namespace SolutionSecrets2019.Commands
{

	internal sealed class PushCommand : CommandAsync
	{

		public const int CommandId = PackageIds.cmdidPushSecrets;

		public static readonly Guid CommandSet = PackageGuids.guidSolutionSecrets2019CmdSet;

		private SolutionSecrets.Core.Commands.PushCommand _pushCommand;


		public static PushCommand Instance {
			get;
			private set;
		}


		public static async Task InitializeAsync(AsyncPackage package)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
			OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
			Instance = new PushCommand(package, commandService);
		}


		private PushCommand(AsyncPackage package, OleMenuCommandService commandService)
			: base(package, commandService)
		{
			commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

			var menuCommandID = new CommandID(CommandSet, CommandId);
			var menuItem = new MenuCommand(this.Execute, menuCommandID);
			commandService.AddCommand(menuItem);

			_pushCommand = new SolutionSecrets.Core.Commands.PushCommand(
				new VSEnvironment(package, commandService)
			);
		}


		private void Execute(object sender, EventArgs e)
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			PushSecretsAsync().Forget();
		}


		private async Task PushSecretsAsync()
		{
			await package.JoinableTaskFactory.SwitchToMainThreadAsync();
			var solutionFullName = SolutionSecrets2019Package._dte.Solution.FullName;

			await _pushCommand.Execute(solutionFullName);

			/*
			SolutionFile solution = new SolutionFile(solutionFullName);

			var secretFiles = solution.GetProjectsSecretFiles();
			if (secretFiles.Count == 0)
			{
				await UseStatusBarAsync("No secrets found.");
				return;
			}

			var synchronizationSettings = solution.CustomSynchronizationSettings;

			// Select the repository for the curront solution
			IRepository repository = Context.Current.GetRepository(synchronizationSettings) ?? Context.Current.Repository;

			await UseStatusBarAsync($"Pushing secrets to {repository.RepositoryTypeFullName} for the solution: {solution.Name} ...");

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
						await UseStatusBarAsync(String.Empty);
						return;
					}
					catch (Exception)
					{
						await UseStatusBarAsync("Error pushing secrets for the solution.");
						return;
					}
				}

				if (!await azureKvRepository.IsReady())
				{
					await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
					System.Windows.MessageBox.Show($"Access denied to Azure Key Vault {azureKvRepository.RepositoryName}.", Vsix.Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);
					commandService.OpenOption<Options.AzureKeyVault.AzureKeyVaultOptionPage>();
					await UseStatusBarAsync("Error pushing secrets for the solution.");
					return;
				}
			}
			else if (!await Context.Current.Cipher.IsReady() || !await repository.IsReady())
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
				System.Windows.MessageBox.Show("You need to configure the solution secrets synchronization before using the Push command.", Vsix.Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);
				commandService.OpenOption<Options.GitHubGists.GitHubGistsOptionPage>();
				await UseStatusBarAsync("Error pushing secrets for the solution.");
				return;
			}

			var headerFile = new HeaderFile
			{
				visualStudioSolutionSecretsVersion = Versions.VersionString,
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
					await UseStatusBarAsync($"Secrets pushed successfully to {repository.RepositoryTypeFullName}.");
				else
					failed = true;
			}

			if (failed)
			{
				await UseStatusBarAsync("Secrets push has failed!");
			}
			*/
		}

	}
}
