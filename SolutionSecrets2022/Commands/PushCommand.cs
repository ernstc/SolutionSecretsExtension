using System.ComponentModel.Design;
using Microsoft.VisualStudio.Threading;


namespace SolutionSecrets2022
{
	[Command(PackageIds.cmdidPushSecrets)]
    internal sealed class PushCommand : BaseCommand<PushCommand>
    {

		private SolutionSecrets.Core.Commands.PushCommand _pushCommand;


		protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
		{
			OleMenuCommandService commandService = await this.Package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
			_pushCommand = new SolutionSecrets.Core.Commands.PushCommand(
				new VSEnvironment(commandService)
				);
			PushSecretsAsync().Forget();
		}


		private async Task PushSecretsAsync()
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			var solutionFullName = SolutionSecrets2022Package._dte.Solution.FullName;

			await _pushCommand.Execute(solutionFullName);
		}

	}
}
