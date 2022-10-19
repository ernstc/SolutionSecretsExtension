using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Windows;

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
			: base(package)
		{
			commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

			var menuCommandID = new CommandID(CommandSet, CommandId);
			var menuItem = new MenuCommand(this.Execute, menuCommandID);
			commandService.AddCommand(menuItem);
		}


		private void Execute(object sender, EventArgs e)
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			PushSecretsAsync().Forget();
		}


		private async Task PushSecretsAsync()
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			var solutionFullName = SolutionSecrets2019Package._dte.Solution.FullName;

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

			if (!await Context.Current.Cipher.IsReady() || !await repository.IsReady())
			{
				System.Windows.MessageBox.Show("You need to configure the solution secrets synchronization before using the Push command.", Vsix.Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);

				var dialog = new ConfigDialog();
				dialog.ShowDialog();

				return;
			}

			await UseStatusBarAsync($"Pushing secrets for solution: {solution.Name} ...");

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
					await UseStatusBarAsync("Secrets pushed successfully.");
				else
					failed = true;
			}

			if (failed)
			{
				await UseStatusBarAsync("Secrets pushing has failed!");
			}
		}

	}
}
