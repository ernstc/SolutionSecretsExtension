using System.IO;
using SolutionSecrets.Core;

namespace SolutionSecrets2022
{
    [Command(PackageIds.cmdidDeleteSecrets)]
    internal sealed class ClearCommand : BaseCommand<ClearCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
			await ClearCommandAsync();
		}


		private async Task ClearCommandAsync()
		{
			var result = await VS.MessageBox.ShowAsync("You are about to clear all the secrets for this solution on this machine.", "Are you sure?",
				icon: Microsoft.VisualStudio.Shell.Interop.OLEMSGICON.OLEMSGICON_WARNING,
				buttons: Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_YESNO,
				defaultButton: Microsoft.VisualStudio.Shell.Interop.OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND);

			if (result != Microsoft.VisualStudio.VSConstants.MessageBoxResult.IDYES)
			{
				await VS.StatusBar.ShowMessageAsync("Solution secrets clearing has been aborted.");
				return;
			}

			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			var solutionFullName = SolutionSecrets2022Package._dte.Solution.FullName;

			SolutionFile solution = new SolutionFile(solutionFullName);

			var secretFiles = solution.GetProjectsSecretFiles();
			if (secretFiles.Count == 0)
			{
				return;
			}

			bool failed = false;
			bool deleted = false;
			foreach (var secretFile in secretFiles)
			{
				FileInfo file = new FileInfo(secretFile.Path);
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
					await VS.StatusBar.ShowMessageAsync("Solution secrets cleared.");
				else
					await VS.StatusBar.ShowMessageAsync("No local secrets found.");
			}
			else
			{
				await VS.StatusBar.ShowMessageAsync("Solution secrets clearing has failed!");
			}
		}

	}
}
