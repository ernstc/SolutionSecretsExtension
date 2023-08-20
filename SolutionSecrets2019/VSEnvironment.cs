using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using EnvDTE;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using SolutionSecrets.Core;
using Task = System.Threading.Tasks.Task;


namespace SolutionSecrets2019
{
	public class VSEnvironment : IEnvironment
	{

		private readonly AsyncPackage _package;
		private readonly OleMenuCommandService _commandService;


		public VSEnvironment(AsyncPackage package, OleMenuCommandService commandService)
		{
			_package = package ?? throw new ArgumentNullException(nameof(package));
			_commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
		}

		public async Task OpenOptionPageAsync(OptionPage optionPage)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			switch (optionPage)
			{
				case OptionPage.AzureKeyVaultOptionPage:
					_commandService.OpenOption<Options.AzureKeyVault.AzureKeyVaultOptionPage>();
					break;
				case OptionPage.GitHubOptionPage:
					_commandService.OpenOption<Options.GitHubGists.GitHubGistsOptionPage>();
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
			await _package.JoinableTaskFactory.SwitchToMainThreadAsync(_package.DisposalToken);
			var dte = await this._package.GetServiceAsync(typeof(DTE)) as DTE;
			Assumes.Present(dte);
			dte.StatusBar.Text = message;
		}

	}
}
