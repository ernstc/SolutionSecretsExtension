using System;
using EnvDTE;

using EnvDTE80;

using Microsoft;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;


namespace SolutionSecrets2019.Commands
{

	public abstract class CommandAsync
    {

		protected readonly AsyncPackage package;
		protected readonly OleMenuCommandService commandService;


		protected Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider {
			get {
				return this.package;
			}
		}


		public CommandAsync(AsyncPackage package, OleMenuCommandService commandService)
		{
			this.package = package ?? throw new ArgumentNullException(nameof(package));
			this.commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
		}


		protected async Task UseStatusBarAsync(string message)
		{
			await package.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
			var dte = await this.package.GetServiceAsync(typeof(DTE)) as DTE;
			Assumes.Present(dte);
			dte.StatusBar.Text = message;
		}
	}
}
