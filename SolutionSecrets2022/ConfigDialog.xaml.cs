using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.Win32;
using SolutionSecrets.Core;
using SolutionSecrets.Core.Repository;
using EnvDTE;
using System.Threading.Tasks;
using SolutionSecrets.Core.Encryption;


namespace SolutionSecrets2022
{
	/// <summary>
	/// Interaction logic for ConfigDialog.xaml
	/// </summary>
	public partial class ConfigDialog : DialogWindow
	{

		const int GITHUB = 0;
		const int AZURE_KV = 1;


		private AsyncPackage _package;
		private OleMenuCommandService _commandService;
		private SolutionFile _solution;


		public ConfigDialog()
		{
			InitializeComponent();
			Loaded += ConfigDialog_Loaded;
		}


		public void Initialize(AsyncPackage package, OleMenuCommandService commandService)
		{
			_package = package;
			_commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

			var solutionFilePath = SolutionSecrets2022Package._dte.Solution.FullName;
			_solution = new SolutionFile(solutionFilePath);

			var defaultSettings = SyncConfiguration.Default;
			var customSettings = SyncConfiguration.GetCustomSynchronizationSettings(_solution.Uid) ?? defaultSettings;
			switch (customSettings.Repository)
			{
				case RepositoryType.GitHub:
					cboxRepositoryType.SelectedIndex = GITHUB;
					gridGitHubGists.Visibility = Visibility.Visible;
					break;

				case RepositoryType.AzureKV:
					cboxRepositoryType.SelectedIndex = AZURE_KV;
					gridAzureKeyVault.Visibility = Visibility.Visible;
					break;
			}

			CheckGitHubRepositoryStatusAsync();
			CheckCipherStatusAsync();

			txtAKVUrl.Text = customSettings.AzureKeyVaultName ?? defaultSettings.AzureKeyVaultName;
			if (!String.IsNullOrWhiteSpace(defaultSettings.AzureKeyVaultName))
			{
				btnAKVResetToDefault.Visibility = String.Equals(txtAKVUrl.Text, defaultSettings.AzureKeyVaultName, StringComparison.OrdinalIgnoreCase) ?
					Visibility.Collapsed :
					Visibility.Visible;
			}
		}


		private async void CheckGitHubRepositoryStatusAsync()
		{
			string status;
			var repository = CoreContext.Current.GetService<IRepository>(nameof(RepositoryType.GitHub));
			await repository.RefreshStatus();
			if (await repository.IsReady())
			{
				status = "Authorized";
			}
			else
			{
				status = "Not authorized";
			}
			await _package.JoinableTaskFactory.SwitchToMainThreadAsync();
			labelGitHubAuthorizationStatus.Text = status;
		}


		private async void CheckCipherStatusAsync()
		{
			string status;
			var cipher = CoreContext.Current.GetService<ICipher>();
			await cipher.RefreshStatus();
			if (await cipher.IsReady())
			{
				status = "Created";
			}
			else
			{
				status = "Not found";
			}
			await _package.JoinableTaskFactory.SwitchToMainThreadAsync();
			labelKeyStatus.Text = status;
		}


		private async void ConfigDialog_Loaded(object sender, RoutedEventArgs e)
		{
			Title = Vsix.Name;
		}


		private async void btnOk_Click(object sender, RoutedEventArgs e)
		{
			if (cboxRepositoryType.SelectedIndex == AZURE_KV)
			{
				var repository = (AzureKeyVaultRepository)CoreContext.Current.GetService<IRepository>(nameof(RepositoryType.AzureKV));
				repository.RepositoryName = txtAKVUrl.Text;
				if (repository.RepositoryName == null)
				{
					System.Windows.MessageBox.Show("The key vault URL is not correct.", Constants.MESSAGE_BOX_TITLE, MessageBoxButton.OK);
					return;
				}
				else if (txtAKVUrl.Text != repository.RepositoryName)
				{
					await _package.JoinableTaskFactory.SwitchToMainThreadAsync();
					txtAKVUrl.Text = repository.RepositoryName;
				}
			}

			if (await ConfigureSolution())
			{
				Close();
			}
		}


		private async Task<bool> ConfigureSolution()
		{
			if (_solution.Uid == Guid.Empty)
			{
				await VS.StatusBar.ShowMessageAsync($"The solution has not a unique identifier. You need to upgrade the solution file.");
				return false;
			}

			var customSettings = SyncConfiguration.GetCustomSynchronizationSettings(_solution.Uid);

			if (cboxRepositoryType.SelectedIndex == GITHUB)
			{
				if (
					customSettings == null
					&& SyncConfiguration.Default.Repository == RepositoryType.GitHub
				)
				{
					return true;
				}

				SyncConfiguration.SetCustomSynchronizationSettings(_solution.Uid, new SolutionSynchronizationSettings
				{
					Repository = RepositoryType.GitHub,
					AzureKeyVaultName = null
				});
				SyncConfiguration.Save();

				await VS.StatusBar.ShowMessageAsync($"Configured GitHub Gist as the repository for the solution secrets.");
				return true;
			}
			else if (cboxRepositoryType.SelectedIndex == AZURE_KV)
			{
				if (
					customSettings == null
					&& SyncConfiguration.Default.Repository == RepositoryType.AzureKV
					&& SyncConfiguration.Default.AzureKeyVaultName == txtAKVUrl.Text
				)
				{
					return true;
				}

				SyncConfiguration.SetCustomSynchronizationSettings(_solution.Uid, new SolutionSynchronizationSettings
				{
					Repository = RepositoryType.AzureKV,
					AzureKeyVaultName = txtAKVUrl.Text
				});
				SyncConfiguration.Save();

				await VS.StatusBar.ShowMessageAsync($"Configured Azure Key Vault as the repository for the solution secrets.");
				return true;
			}
			return false;
		}


		private void cboxRepositoryType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			switch (cboxRepositoryType.SelectedIndex)
			{
				case GITHUB:
					gridAzureKeyVault.Visibility = Visibility.Collapsed;
					gridGitHubGists.Visibility = Visibility.Visible;
					break;

				case AZURE_KV:
					gridGitHubGists.Visibility = Visibility.Collapsed;
					gridAzureKeyVault.Visibility = Visibility.Visible;
					break;
			}
		}


		private void btnAKVResetToDefault_Click(object sender, RoutedEventArgs e)
		{
			var defaultSettings = SyncConfiguration.Default;
			if (!String.IsNullOrWhiteSpace(defaultSettings.AzureKeyVaultName))
			{
				txtAKVUrl.Text = defaultSettings.AzureKeyVaultName;
			}
		}


		private void btnOptions_Click(object sender, RoutedEventArgs e)
		{
			switch (cboxRepositoryType.SelectedIndex)
			{
				case GITHUB:
					Close();
					_commandService.OpenOption<Options.GitHubGists.GitHubGistsOptionPage>();
					break;

				case AZURE_KV:
					Close();
					_commandService.OpenOption<Options.AzureKeyVault.AzureKeyVaultOptionPage>();
					break;
			}
		}


		private void txtAKVUrl_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			var defaultSettings = SyncConfiguration.Default;
			if (!String.IsNullOrWhiteSpace(defaultSettings.AzureKeyVaultName))
			{
				btnAKVResetToDefault.Visibility = String.Equals(txtAKVUrl.Text, defaultSettings.AzureKeyVaultName, StringComparison.OrdinalIgnoreCase) ?
					Visibility.Collapsed :
					Visibility.Visible;
			}
		}

	}
}
