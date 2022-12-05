using System;
using System.ComponentModel.Design;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using SolutionSecrets.Core;

namespace SolutionSecrets2019
{
	/// <summary>
	/// Interaction logic for ConfigDialog.xaml
	/// </summary>
	public partial class ConfigDialog : DialogWindow
	{

		private bool _isLoaded;
		private OleMenuCommandService _commandService;


		public ConfigDialog()
		{
			InitializeComponent();
			Loaded += ConfigDialog_Loaded;
		}


		public virtual void Initialize(OleMenuCommandService commandService)
		{
			_commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

			switch (SyncConfiguration.Default.Repository)
			{
				case SolutionSecrets.Core.Repository.RepositoryType.GitHub:
					cboxRepositoryType.SelectedIndex = 0;
					gridGitHubGists.Visibility = Visibility.Visible;
					break;
				case SolutionSecrets.Core.Repository.RepositoryType.AzureKV:
					cboxRepositoryType.SelectedIndex = 1;
					gridAzureKeyVault.Visibility = Visibility.Visible;
					break;
			}
		}


		private async void ConfigDialog_Loaded(object sender, RoutedEventArgs e)
		{
			_isLoaded = true;
			Title = Vsix.Name;
		}


		private async void btnOk_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}


		private void cboxRepositoryType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			switch (cboxRepositoryType.SelectedIndex)
			{
				case 0:
					gridAzureKeyVault.Visibility = Visibility.Collapsed;
					gridGitHubGists.Visibility = Visibility.Visible;
					break;
				case 1:
					gridGitHubGists.Visibility = Visibility.Collapsed;
					gridAzureKeyVault.Visibility = Visibility.Visible;
					break;
			}
		}


		private void btnConfigureGitHubGists_Click(object sender, RoutedEventArgs e)
		{
			Guid cmdGroup = typeof(VSConstants.VSStd97CmdID).GUID;
			var cmd = new CommandID(cmdGroup, VSConstants.cmdidToolsOptions);
			_commandService.GlobalInvoke(cmd, typeof(Options.GitHubGists.GitHubGistsOptionPage).GUID.ToString());
		}

		private void btnAKVChangeDefaultUrl_Click(object sender, RoutedEventArgs e)
		{
			Guid cmdGroup = typeof(VSConstants.VSStd97CmdID).GUID;
			var cmd = new CommandID(cmdGroup, VSConstants.cmdidToolsOptions);
			_commandService.GlobalInvoke(cmd, typeof(Options.AzureKeyVault.AzureKeyVaultOptionPage).GUID.ToString());
		}

		private void btnAKVResetToDefault_Click(object sender, RoutedEventArgs e)
		{
			if (!String.IsNullOrWhiteSpace(txtAKVDefaultUrl.Text))
			{
				txtAKVUrl.Text = txtAKVDefaultUrl.Text;
			}
		}
	}
}
