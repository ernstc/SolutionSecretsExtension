using System;
using System.ComponentModel.Design;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using EnvDTE;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using SolutionSecrets.Core;
using Task = System.Threading.Tasks.Task;


namespace SolutionSecrets2019
{
	/// <summary>
	/// Interaction logic for ConfigDialog.xaml
	/// </summary>
	public partial class ConfigDialog : DialogWindow
	{

		private bool _isLoaded;
		private AsyncPackage _package;
		private OleMenuCommandService _commandService;


		public ConfigDialog()
		{
			InitializeComponent();
			Loaded += ConfigDialog_Loaded;
		}


		public virtual void Initialize(AsyncPackage package, OleMenuCommandService commandService)
		{
			_package = package;
			_commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

			var solutionFilePath = SolutionSecrets2019Package._dte.Solution.FullName;
			SolutionFile solution = new SolutionFile(solutionFilePath);

			var customSettings = SyncConfiguration.GetCustomSynchronizationSettings(solution.Uid);
			if (customSettings == null)
			{

				switch (SyncConfiguration.Default.Repository)
				{
					case SolutionSecrets.Core.Repository.RepositoryType.GitHub:
						{
							cboxRepositoryType.SelectedIndex = 0;
							gridGitHubGists.Visibility = Visibility.Visible;
							CheckRepositoryStatusAsync();
							CheckCipherStatusAsync();
							break;
						}
					case SolutionSecrets.Core.Repository.RepositoryType.AzureKV:
						{
							cboxRepositoryType.SelectedIndex = 1;
							gridAzureKeyVault.Visibility = Visibility.Visible;
							txtAKVDefaultUrl.Text = SyncConfiguration.Default.AzureKeyVaultName;
							break;
						}
				}
				btnResetToDefaults.Visibility = Visibility.Collapsed;
			}
			else
			{
				switch (customSettings.Repository)
				{
					case SolutionSecrets.Core.Repository.RepositoryType.GitHub:
						{
							cboxRepositoryType.SelectedIndex = 0;
							gridGitHubGists.Visibility = Visibility.Visible;
							CheckRepositoryStatusAsync();
							CheckCipherStatusAsync();
							break;
						}
					case SolutionSecrets.Core.Repository.RepositoryType.AzureKV:
						{
							cboxRepositoryType.SelectedIndex = 1;
							gridAzureKeyVault.Visibility = Visibility.Visible;
							txtAKVDefaultUrl.Text = SyncConfiguration.Default.AzureKeyVaultName;
							txtAKVUrl.Text = customSettings.AzureKeyVaultName ?? SyncConfiguration.Default.AzureKeyVaultName;
							break;
						}
				}
				btnResetToDefaults.Visibility = Visibility.Visible;
			}
		}


		private async void CheckRepositoryStatusAsync()
		{
			await Services.Repository.RefreshStatus();
			if (await Services.Repository.IsReady())
			{
				labelGitHubAuthorizationStatus.Content = "Authorized";
			}
			else
			{
				labelGitHubAuthorizationStatus.Content = "Not authorized";
			}
		}


		private async void CheckCipherStatusAsync()
		{
			await Services.Cipher.RefreshStatus();
			if (await Services.Cipher.IsReady())
			{
				labelKeyStatus.Content = "Created";
			}
			else
			{
				labelKeyStatus.Content = "Not found";
			}
		}


		private async void ConfigDialog_Loaded(object sender, RoutedEventArgs e)
		{
			_isLoaded = true;
			Title = Vsix.Name;
		}


		private async void btnOk_Click(object sender, RoutedEventArgs e)
		{
			if (await ConfigureSolution(reset: false))
			{
				Close();
			}
		}


		protected async Task UseStatusBarAsync(string message)
		{
			await _package.JoinableTaskFactory.SwitchToMainThreadAsync(_package.DisposalToken);
			var dte = await this._package.GetServiceAsync(typeof(DTE)) as DTE;
			Assumes.Present(dte);
			dte.StatusBar.Text = message;
		}


		private async Task<bool> ConfigureSolution(bool reset)
		{
			var solutionFilePath = SolutionSecrets2019Package._dte.Solution.FullName;
			SolutionFile solution = new SolutionFile(solutionFilePath);

			if (solution.Uid == Guid.Empty)
			{
				await UseStatusBarAsync($"The solution has not a unique identifier. You need to upgrade the solution file.");
				return false;
			}

			if (reset)
			{
				SyncConfiguration.SetCustomSynchronizationSettings(solution.Uid, null);
				SyncConfiguration.Save();

				await UseStatusBarAsync($"Removed custom configuration for the solution secrets.");
				return true;
			}

			var customSettings = SyncConfiguration.GetCustomSynchronizationSettings(solution.Uid);

			if (cboxRepositoryType.SelectedIndex == 0)
			{
				if (
					customSettings == null
					&& SyncConfiguration.Default.Repository == SolutionSecrets.Core.Repository.RepositoryType.GitHub
					)
				{
					return true;
				}

				SyncConfiguration.SetCustomSynchronizationSettings(solution.Uid, new SolutionSynchronizationSettings
				{
					Repository = SolutionSecrets.Core.Repository.RepositoryType.GitHub,
					AzureKeyVaultName = null
				});
				SyncConfiguration.Save();

				await UseStatusBarAsync($"Configured GitHub Gist as the repository for the solution secrets.");
				return true;
			}
			else if (cboxRepositoryType.SelectedIndex == 1)
			{
				if (
					customSettings == null
					&& SyncConfiguration.Default.Repository == SolutionSecrets.Core.Repository.RepositoryType.AzureKV
					&& SyncConfiguration.Default.AzureKeyVaultName == txtAKVUrl.Text
					)
				{
					return true;
				}

				SyncConfiguration.SetCustomSynchronizationSettings(solution.Uid, new SolutionSynchronizationSettings
				{
					Repository = SolutionSecrets.Core.Repository.RepositoryType.AzureKV,
					AzureKeyVaultName = txtAKVUrl.Text
				});
				SyncConfiguration.Save();

				await UseStatusBarAsync($"Configured Azure Key Vault as the repository for the solution secrets.");
				return true;
			}
			return false;
		}


		private void cboxRepositoryType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			var solutionFilePath = SolutionSecrets2019Package._dte.Solution.FullName;
			SolutionFile solution = new SolutionFile(solutionFilePath);

			var customSettings = SyncConfiguration.GetCustomSynchronizationSettings(solution.Uid);

			SolutionSecrets.Core.Repository.RepositoryType repositoryType = default;
			switch (cboxRepositoryType.SelectedIndex)
			{
				case 0:
					repositoryType = SolutionSecrets.Core.Repository.RepositoryType.GitHub;
					gridAzureKeyVault.Visibility = Visibility.Collapsed;
					gridGitHubGists.Visibility = Visibility.Visible;
					break;

				case 1:
					repositoryType = SolutionSecrets.Core.Repository.RepositoryType.AzureKV;
					gridGitHubGists.Visibility = Visibility.Collapsed;
					gridAzureKeyVault.Visibility = Visibility.Visible;
					txtAKVDefaultUrl.Text = SyncConfiguration.Default.AzureKeyVaultName;

					if (customSettings != null)
					{
						txtAKVUrl.Text = customSettings.AzureKeyVaultName;
						btnResetToDefaults.Visibility = Visibility.Visible;
					}
					else
					{
						txtAKVUrl.Text = txtAKVDefaultUrl.Text;
						btnResetToDefaults.Visibility = Visibility.Collapsed;
					}
					break;
			}

			if (customSettings != null && customSettings.Repository != repositoryType)
			{
				btnResetToDefaults.Visibility = Visibility.Visible;
			}
		}


		private void btnConfigureGitHubGists_Click(object sender, RoutedEventArgs e)
		{
			Close();
			_commandService.OpenOption<Options.GitHubGists.GitHubGistsOptionPage>();
		}

		private void btnAKVChangeDefaultUrl_Click(object sender, RoutedEventArgs e)
		{
			Close();
			_commandService.OpenOption<Options.AzureKeyVault.AzureKeyVaultOptionPage>();
		}


		private void btnAKVResetToDefault_Click(object sender, RoutedEventArgs e)
		{
			if (!String.IsNullOrWhiteSpace(txtAKVDefaultUrl.Text))
			{
				txtAKVUrl.Text = txtAKVDefaultUrl.Text;
			}
		}


		private async void btnResetToDefaults_Click(object sender, RoutedEventArgs e)
		{
			if (await ConfigureSolution(reset: true))
			{
				btnResetToDefaults.Visibility = Visibility.Collapsed;
				txtAKVUrl.Text = txtAKVDefaultUrl.Text;
				Initialize(_package, _commandService);
			}
		}

	}
}
