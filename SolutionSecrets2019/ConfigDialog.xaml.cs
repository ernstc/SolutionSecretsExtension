using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.PlatformUI;


namespace SolutionSecrets2019
{
	/// <summary>
	/// Interaction logic for ConfigDialog.xaml
	/// </summary>
	public partial class ConfigDialog : DialogWindow
	{

		private bool _isLoaded;
		private FileInfo _selectedFile;
		private bool _generatingNewEncryptionKey;


		public ConfigDialog()
		{
			InitializeComponent();
			Loaded += ConfigDialog_Loaded;
		}


		private async void ConfigDialog_Loaded(object sender, RoutedEventArgs e)
		{
			_isLoaded = true;
			_generatingNewEncryptionKey = false;

			//Icon = BitmapFrame.Create(new Uri("pack://application:,,,/SolutionSecrets2019;component/Resources/Icon.png", UriKind.RelativeOrAbsolute));
			Title = Vsix.Name;

			await CheckCipherStatusAsync();
			await CheckRepositoryStatusAsync();
		}


		private async Task CheckRepositoryStatusAsync()
		{
			await Services.Repository.RefreshStatus();
			if (await Services.Repository.IsReady())
			{
				panelGitHubAuthorizationStatus.Visibility = Visibility.Visible;
			}
			else
			{
				btnAuthorizeGitHub.Visibility = Visibility.Visible;
			}
		}


		private async Task CheckCipherStatusAsync()
		{
			await Services.Cipher.RefreshStatus();
			if (await Services.Cipher.IsReady())
			{
				panelCreateKey.Visibility = Visibility.Collapsed;
				panelKeyStatus.Visibility = Visibility.Visible;
				_generatingNewEncryptionKey = false;
			}
			else
			{
				panelKeyStatus.Visibility = Visibility.Collapsed;
				panelCreateKey.Visibility = Visibility.Visible;
				txtPassPhrase.Focus();
				_generatingNewEncryptionKey = true;
			}
		}


		private void btnCreateNewKey_Click(object sender, RoutedEventArgs e)
		{
			panelKeyStatus.Visibility = Visibility.Collapsed;
			panelCreateKey.Visibility = Visibility.Visible;
			txtPassPhrase.Focus();
			_generatingNewEncryptionKey = true;
		}


		private void cbPassPhrase_Checked(object sender, RoutedEventArgs e)
		{
			if (_isLoaded)
			{
				txtKeyFilePath.IsEnabled = false;
				txtKeyFilePath.Text = null;
				txtKeyFilePath.Tag = null;
				btnBrowseFile.IsEnabled = false;

				txtPassPhrase.IsEnabled = true;
				txtConfirmPassPhrase.IsEnabled = true;
			}
		}


		private void cbKeyFilePath_Checked(object sender, RoutedEventArgs e)
		{
			if (_isLoaded)
			{
				txtPassPhrase.Password = null;
				txtPassPhrase.IsEnabled = false;
				txtConfirmPassPhrase.Password = null;
				txtConfirmPassPhrase.IsEnabled = false;

				txtKeyFilePath.IsEnabled = true;
				btnBrowseFile.IsEnabled = true;
			}
		}


		private void btnBrowseFile_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				_selectedFile = new FileInfo(openFileDialog.FileName);
				txtKeyFilePath.Text = _selectedFile.Name;
				txtKeyFilePath.Tag = openFileDialog.FileName;
			}
		}


		private async void btnAuthorizeGitHub_Click(object sender, RoutedEventArgs e)
		{
			btnAuthorizeGitHub.IsEnabled = false;

			string deviceCode = await Services.Repository.StartDeviceFlowAuthorizationAsync();
			if (deviceCode == null)
			{
				return;
			}

			txtDeviceCode.Text = deviceCode;
			btnAuthorizeGitHub.Visibility = Visibility.Collapsed;
			btnAuthorizeGitHub.IsEnabled = true;
			panelAuthorizingGitHub.Visibility = Visibility.Visible;
		}


		private async void btnContinueAuthorizingGitHub_Click(object sender, RoutedEventArgs e)
		{
			btnContinueAuthorizingGitHub.IsEnabled = false;

			await Services.Repository.CompleteDeviceFlowAuthorizationAsync();
			if (await Services.Repository.IsReady())
			{
				panelAuthorizingGitHub.Visibility = Visibility.Collapsed;
				panelGitHubAuthorizationStatus.Visibility = Visibility.Visible;
			}
			else
			{
				panelAuthorizingGitHub.Visibility = Visibility.Collapsed;
				btnAuthorizeGitHub.Visibility = Visibility.Visible;
			}

			btnContinueAuthorizingGitHub.IsEnabled = true;
		}


		private async void btnOk_Click(object sender, RoutedEventArgs e)
		{
			if (_generatingNewEncryptionKey)
			{
				if (await Services.Cipher.IsReady())
				{
					MessageBoxResult result = System.Windows.MessageBox.Show("You are updating the encryption key.\nSaved secrets will no longer be recoverable.\n\nAre you sure?", Constants.MESSAGE_BOX_TITLE, MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
					if (result != MessageBoxResult.Yes)
					{
						Close();
						return;
					}
				}

				Func<bool> action = txtPassPhrase.IsEnabled ? 
					new Func<bool>(GenerateKeyFromPassphrase) : 
					new Func<bool>(GenerateKeyFromFile);

				if (action())
				{
					await CheckCipherStatusAsync();
					txtPassPhrase.Password = null;
					txtConfirmPassPhrase.Password = null;
					txtKeyFilePath.Text = null;
					txtKeyFilePath.Tag = null;
					Close();
				}
				else
				{
					return;
				}
			}
			Close();
		}


		#region Cipher routines

		private bool GenerateKeyFromPassphrase()
		{
			if (txtPassPhrase.Password != txtConfirmPassPhrase.Password)
			{
				System.Windows.MessageBox.Show("The passphrase has not been confirmed correctly.", Constants.MESSAGE_BOX_TITLE, MessageBoxButton.OK);
				return false;
			}

			var validationError = ValidatePassphrase(txtPassPhrase.Password);
			if (validationError != null)
			{
				System.Windows.MessageBox.Show(validationError, Constants.MESSAGE_BOX_TITLE, MessageBoxButton.OK);
				return false;
			}

			Services.Cipher.Init(txtPassPhrase.Password);
			return true;
		}


		private bool GenerateKeyFromFile()
		{
			string filePath = txtKeyFilePath.Tag as string;

			if (String.IsNullOrEmpty(filePath))
			{
				System.Windows.MessageBox.Show("Select a key file.", Constants.MESSAGE_BOX_TITLE, MessageBoxButton.OK);
				return false;
			}

			if (!File.Exists(filePath))
			{
				System.Windows.MessageBox.Show("The selected file does not exist.", Constants.MESSAGE_BOX_TITLE, MessageBoxButton.OK);
				return false;
			}

			using (var file = File.OpenRead(filePath))
			{
				Services.Cipher.Init(file);
			}
			return true;
		}


		public string ValidatePassphrase(string passphrase)
		{
			if (String.IsNullOrWhiteSpace(passphrase))
			{
				return "The passphrase is empty.";
			}

			var hasNumber = new Regex(@"[0-9]+");
			var hasUpperChar = new Regex(@"[A-Z]+");
			var hasMiniMaxChars = new Regex(@".{8,}");
			var hasLowerChar = new Regex(@"[a-z]+");
			var hasSymbols = new Regex(@"[!@#$%^&*()_+=\[{\]};:<>|./?,-]");

			bool isValid =
				hasLowerChar.IsMatch(passphrase)
				&& hasUpperChar.IsMatch(passphrase)
				&& hasMiniMaxChars.IsMatch(passphrase)
				&& hasNumber.IsMatch(passphrase)
				&& hasSymbols.IsMatch(passphrase);

			if (!isValid)
				return "The passphrase is weak. It should contains at least 8 characters in upper and lower case, at least one digit and at least one symbol between !@#$%^&*()_+=[{]};:<>|./?,-";

			return null;
		}

		#endregion

	}
}
