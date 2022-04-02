using System.IO;
using SolutionSecrets.Core;

namespace SolutionSecrets2022
{
    [Command(PackageIds.cmdidDeleteSecrets)]
    internal sealed class DeleteLocallyCommand : BaseCommand<DeleteLocallyCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
			await DeleteLocallyAsync();
		}


		private async Task DeleteLocallyAsync()
		{
			var result = await VS.MessageBox.ShowAsync("You are about to locally delete all the secrets for this solution.", "Are you sure?",
				icon: Microsoft.VisualStudio.Shell.Interop.OLEMSGICON.OLEMSGICON_WARNING,
				buttons: Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_YESNO,
				defaultButton: Microsoft.VisualStudio.Shell.Interop.OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND);

			if (result != Microsoft.VisualStudio.VSConstants.MessageBoxResult.IDYES)
			{
				await VS.StatusBar.ShowMessageAsync($"Solution secrets deletetion has been aborted.");
				return;
			}

			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			var solutionFullName = SolutionSecrets2022Package._dte.Solution.FullName;

			SolutionFile solution = new SolutionFile(solutionFullName);

			var configFiles = solution.GetProjectsSecretConfigFiles();
			if (configFiles.Count == 0)
			{
				return;
			}

			bool failed = false;
			foreach (var configFile in configFiles)
			{
				FileInfo file = new FileInfo(configFile.FullName);
				if (file.Directory.Exists)
				{
					try
					{
						file.Directory.Delete(true);
					}
					catch
					{
						failed = true;
					}
				}
			}

			if (!failed)
				await VS.StatusBar.ShowMessageAsync("Solution secrets deleted locally.");
			else
				await VS.StatusBar.ShowMessageAsync("Local deletion of solution secrets failed!");
		}
	}
}
