using System;
using System.ComponentModel.Design;
using System.Globalization;

using Microsoft.VisualStudio.Shell;

using Task = System.Threading.Tasks.Task;


namespace SolutionSecrets2019.Commands
{

	internal sealed class ConfigureCommand : CommandAsync
	{

		public const int CommandId = PackageIds.cmdidInitSecrestsSync;

		public static readonly Guid CommandSet = PackageGuids.guidSolutionSecrets2019CmdSet;



		public static ConfigureCommand Instance {
			get;
			private set;
		}


		public static async Task InitializeAsync(AsyncPackage package)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
			OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
			Instance = new ConfigureCommand(package, commandService);
		}


		private ConfigureCommand(AsyncPackage package, OleMenuCommandService commandService)
			: base(package, commandService)
		{
			commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

			var menuCommandID = new CommandID(CommandSet, CommandId);
			var menuItem = new MenuCommand(this.Execute, menuCommandID);
			commandService.AddCommand(menuItem);
		}


		private void Execute(object sender, EventArgs e)
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			var dialog = new ConfigDialog();
			dialog.Initialize(commandService);
			dialog.ShowDialog();
		}

	}
}
