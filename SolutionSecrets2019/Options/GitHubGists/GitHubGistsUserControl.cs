using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using SolutionSecrets.Core;
using SolutionSecrets.Core.Repository;

namespace SolutionSecrets2019.Options.GitHubGists
{
	internal partial class GitHubGistsUserControl : UserControl
	{
		internal GitHubGistsOptionPage optionsPage;

		private bool _isLoaded;
		private FileInfo _selectedFile;
		private bool _generatingNewEncryptionKey;


		public async Task InitializeAsync()
		{
			_isLoaded = true;
			_generatingNewEncryptionKey = false;

			await CheckCipherStatusAsync();
			await CheckRepositoryStatusAsync();
		}


		public GitHubGistsUserControl()
		{
			InitializeComponent();
		}



		#region Authorization routines

		private async void btnAuthorize_Click(object sender, EventArgs e)
		{
			btnAuthorize.Enabled = false;

			string deviceCode = await Services.Repository.StartDeviceFlowAuthorizationAsync();
			if (deviceCode == null)
			{
				return;
			}

			txtDeviceCode.Text = deviceCode;
			txtDeviceCode.Focus();
			txtDeviceCode.SelectAll();
			txtDeviceCode.Copy();

			btnAuthorize.Visible = false;
			pnlContinueAuthorization.Visible = true;
		}


		private void btnResetAuthorization_Click(object sender, EventArgs e)
		{
			AppData.SaveData(GistRepository.APP_DATA_FILENAME, new GistRepository.RepositoryAppData
			{
				access_token = String.Empty
			});
			SetNotAuthorized();
		}


		private async void btnContinueAuthorization_Click(object sender, EventArgs e)
		{
			btnContinueAuthorization.Enabled = false;

			await Services.Repository.CompleteDeviceFlowAuthorizationAsync();
			await Services.Repository.RefreshStatus();
			if (await Services.Repository.IsReady())
			{
				SetAuthorized();
			}
		}


		private void btnUndoCompleteAuthorization_Click(object sender, EventArgs e)
		{
			Services.Repository.AbortAuthorization();
			SetNotAuthorized();
		}


		private async Task CheckRepositoryStatusAsync()
		{
			if (await Services.Repository.IsReady())
			{
				SetAuthorized();
			}
			else
			{
				SetNotAuthorized();
			}
		}


		private void SetAuthorized()
		{
			lblAuthorizationStatus.Text = " Authorized ";
			btnAuthorize.Visible = false;
			btnResetAuthorization.Visible = true;
			pnlContinueAuthorization.Visible = false;
		}


		private void SetNotAuthorized()
		{
			lblAuthorizationStatus.Text = " Not authorized ";
			btnAuthorize.Enabled = true;
			btnAuthorize.Visible = true;
			btnResetAuthorization.Visible = false;
			pnlContinueAuthorization.Visible = false;
			btnContinueAuthorization.Enabled = true;
		}

		#endregion



		#region Cipher routines

		private void btnChangeKey_Click(object sender, EventArgs e)
		{
			pnlCreateEncryptionKey.Visible = true;
			btnGenerateKey.Enabled = true;
			btnUndoGenerateKey.Enabled = true;
			txtPassphrase.Focus();
			_generatingNewEncryptionKey = true;
		}


		private void rbtnPassphrase_CheckedChanged(object sender, EventArgs e)
		{
			if (_isLoaded)
			{
				txtKeyFilePath.Enabled = false;
				txtKeyFilePath.Text = null;
				txtKeyFilePath.Tag = null;
				btnBrowseKeyFile.Enabled = false;

				txtPassphrase.Enabled = true;
				txtConfirmPassphrase.Enabled = true;
			}
		}


		private void rbtnKeyFile_CheckedChanged(object sender, EventArgs e)
		{
			if (_isLoaded)
			{
				txtPassphrase.Text = null;
				txtPassphrase.Enabled = false;
				txtConfirmPassphrase.Text = null;
				txtConfirmPassphrase.Enabled = false;

				txtKeyFilePath.Enabled = true;
				btnBrowseKeyFile.Enabled = true;
			}
		}


		private void btnBrowseKeyFile_Click(object sender, EventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				_selectedFile = new FileInfo(openFileDialog.FileName);
				txtKeyFilePath.Text = _selectedFile.Name;
				txtKeyFilePath.Tag = openFileDialog.FileName;
			}
		}


		private async void btnGenerateKey_Click(object sender, EventArgs e)
		{
			btnGenerateKey.Enabled = false;
			btnUndoGenerateKey.Enabled = false;
			if (await GenerateKeyAsync())
			{
				await CheckCipherStatusAsync();
			}
			else
			{
				btnGenerateKey.Enabled = true;
				btnUndoGenerateKey.Enabled = true;
			}
		}


		private void btnUndoGenerateKey_Click(object sender, EventArgs e)
		{
			txtPassphrase.Text = null;
			txtConfirmPassphrase.Text = null;
			txtKeyFilePath.Text = null;
			txtKeyFilePath.Tag = null;
			pnlCreateEncryptionKey.Visible = false;
		}


		private async Task CheckCipherStatusAsync()
		{
			await Services.Cipher.RefreshStatus();
			if (await Services.Cipher.IsReady())
			{
				lblEncryptionKeyStatus.Text = " Created ";
				pnlCreateEncryptionKey.Visible = false;
				_generatingNewEncryptionKey = false;
			}
			else
			{
				lblEncryptionKeyStatus.Text = " Not found ";
				pnlCreateEncryptionKey.Visible = true;
				txtPassphrase.Focus();
				_generatingNewEncryptionKey = true;
			}
		}


		private async Task<bool> GenerateKeyAsync()
		{
			if (_generatingNewEncryptionKey)
			{
				if (await Services.Cipher.IsReady())
				{
					MessageBoxResult result = System.Windows.MessageBox.Show("You are updating the encryption key.\nSaved secrets will no longer be recoverable.\n\nAre you sure?", Constants.MESSAGE_BOX_TITLE, MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
					if (result != MessageBoxResult.Yes)
					{
						return false;
					}
				}

				Func<bool> action = txtPassphrase.Enabled ?
					new Func<bool>(GenerateKeyFromPassphrase) :
					new Func<bool>(GenerateKeyFromFile);

				if (action())
				{
					await CheckCipherStatusAsync();
					txtPassphrase.Text = null;
					txtConfirmPassphrase.Text = null;
					txtKeyFilePath.Text = null;
					txtKeyFilePath.Tag = null;
					return true;
				}
				else
				{
					return false;
				}
			}
			return false;
		}


		private bool GenerateKeyFromPassphrase()
		{
			if (txtPassphrase.Text != txtConfirmPassphrase.Text)
			{
				System.Windows.MessageBox.Show("The passphrase has not been confirmed correctly.", Constants.MESSAGE_BOX_TITLE, MessageBoxButton.OK);
				return false;
			}

			var validationError = ValidatePassphrase(txtPassphrase.Text);
			if (validationError != null)
			{
				System.Windows.MessageBox.Show(validationError, Constants.MESSAGE_BOX_TITLE, MessageBoxButton.OK);
				return false;
			}

			Services.Cipher.Init(txtPassphrase.Text);
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
