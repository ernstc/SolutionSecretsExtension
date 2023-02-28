using System.ComponentModel.Design;

namespace SolutionSecrets2022
{
    [Command(PackageIds.cmdidConfigureCommand)]
    internal sealed class ConfigureCommand : BaseCommand<ConfigureCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
			OleMenuCommandService commandService = 
				await this.Package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;

			var dialog = new ConfigDialog();
			dialog.Initialize(this.Package, commandService);
			dialog.ShowDialog();
		}
    }
}
