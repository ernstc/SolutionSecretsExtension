using System.ComponentModel.Design;
using Microsoft.VisualStudio.Threading;


namespace SolutionSecrets2022
{
	[Command(PackageIds.cmdidPullSecrets)]
    internal sealed class PullCommand : BaseCommand<PullCommand>
    {

		private SolutionSecrets.Core.Commands.PullCommand _pullCommand;


		protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
		{
			OleMenuCommandService commandService = await this.Package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
			_pullCommand = new SolutionSecrets.Core.Commands.PullCommand(
				new VSEnvironment(commandService)
				);
			PullSecretsAsync().Forget();
		}


		private async Task PullSecretsAsync()
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			var solutionFullName = SolutionSecrets2022Package._dte.Solution.FullName;

			await _pullCommand.Execute(solutionFullName);
		}

	}
}
