namespace SolutionSecrets2022
{
    [Command(PackageIds.cmdidConfigureCommand)]
    internal sealed class ConfigureCommand : BaseCommand<ConfigureCommand>
    {
        protected override Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
			var dialog = new ConfigDialog();
			dialog.ShowDialog();
			return Task.CompletedTask;
		}
    }
}
