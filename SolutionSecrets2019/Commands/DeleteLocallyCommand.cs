using System;
using System.ComponentModel.Design;
using System.IO;
using System.Windows;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;

using SolutionSecrets.Core;

using Task = System.Threading.Tasks.Task;


namespace SolutionSecrets2019.Commands
{

	internal sealed class DeleteLocallyCommand : CommandAsync
	{

		public const int CommandId = PackageIds.cmdidDeleteSecrets;

		public static readonly Guid CommandSet = PackageGuids.guidSolutionSecrets2019CmdSet;


		public static DeleteLocallyCommand Instance {
			get;
			private set;
		}


		public static async Task InitializeAsync(AsyncPackage package)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
			OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
			Instance = new DeleteLocallyCommand(package, commandService);
		}


		private DeleteLocallyCommand(AsyncPackage package, OleMenuCommandService commandService)
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
			DeleteLocallyAsync().Forget();
		}


		private async Task DeleteLocallyAsync()
		{
			MessageBoxResult result = System.Windows.MessageBox.Show("You are about to locally delete all the secrets for this solution.\nAre you sure?", Constants.MESSAGE_BOX_TITLE, MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
			if (result != MessageBoxResult.Yes)
			{
				await UseStatusBarAsync($"Solution secrets deletetion has been aborted.");
				return;
			}

			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			var solutionFullName = SolutionSecrets2019Package._dte.Solution.FullName;

			SolutionFile solution = new SolutionFile(solutionFullName);

			var configFiles = solution.GetProjectsSecretConfigFiles();
			if (configFiles.Count == 0)
			{
				return;
			}

			bool failed = false;
			bool deleted = false;
			foreach (var configFile in configFiles)
			{
				FileInfo file = new FileInfo(configFile.FilePath);
				if (file.Directory.Exists)
				{
					try
					{
						file.Directory.Delete(true);
						deleted = true;
					}
					catch
					{
						failed = true;
					}
				}
			}

			if (!failed)
			{
				if (deleted)
					await UseStatusBarAsync("Solution secrets deleted locally.");
				else
					await UseStatusBarAsync("No local secrets found.");
			}
			else
				await UseStatusBarAsync("Local deletion of solution secrets failed!");
		}

	}
}
