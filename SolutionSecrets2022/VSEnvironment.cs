using System.Windows;
using SolutionSecrets.Core;

namespace SolutionSecrets2022
{
	internal class VSEnvironment : IEnvironment
	{

		private readonly OleMenuCommandService _commandService;

		public VSEnvironment(OleMenuCommandService commandService)
		{
			_commandService = commandService;
		}

		public async Task OpenOptionPageAsync(OptionPage optionPage)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			switch (optionPage)
			{
				case OptionPage.GitHubOptionPage:
					_commandService.OpenOption<Options.GitHubGists.GitHubGistsOptionPage>();
					break;
				case OptionPage.AzureKeyVaultOptionPage:
					_commandService.OpenOption<Options.AzureKeyVault.AzureKeyVaultOptionPage>();
					break;
			}
		}

		public async Task ShowDialogMessageAsync(string message)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();   
			System.Windows.MessageBox.Show(message, Vsix.Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);
		}

		public async Task ShowStatusMessageAsync(string message)
		{
			await VS.StatusBar.ShowMessageAsync(message);
		}
	}
}
