using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;

using Task = System.Threading.Tasks.Task;


namespace SolutionSecrets2019.Commands
{

	internal sealed class OptionsCommand : CommandAsync
	{

		public const int CommandId = PackageIds.cmdidGlobalOptions;

		public static readonly Guid CommandSet = PackageGuids.guidSolutionSecrets2019CmdSet;

		private readonly OleMenuCommandService _commandService;


		public static OptionsCommand Instance {
			get;
			private set;
		}


		public static async Task InitializeAsync(AsyncPackage package)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
			OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
			Instance = new OptionsCommand(package, commandService);
		}


		private OptionsCommand(AsyncPackage package, OleMenuCommandService commandService)
			: base(package)
		{
			_commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

			var menuCommandID = new CommandID(CommandSet, CommandId);
			var menuItem = new MenuCommand(this.Execute, menuCommandID);
			commandService.AddCommand(menuItem);
		}


		private void Execute(object sender, EventArgs e)
		{
			Guid cmdGroup = typeof(VSConstants.VSStd97CmdID).GUID;
			var cmd = new CommandID(cmdGroup, VSConstants.cmdidToolsOptions);
			_commandService.GlobalInvoke(cmd, typeof(Options.General.GeneralOptionPage).GUID.ToString());
		}

	}
}
