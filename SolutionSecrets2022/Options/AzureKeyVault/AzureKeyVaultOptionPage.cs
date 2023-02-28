using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;
using SolutionSecrets.Core;
using SolutionSecrets.Core.Repository;


namespace SolutionSecrets2022.Options.AzureKeyVault
{
	[ComVisible(true)]
	internal class AzureKeyVaultOptionPage : DialogPage
	{

		private AzureKeyVaultUserControl _page;


		public AzureKeyVaultOptionPage()
		{
			_page = new AzureKeyVaultUserControl();
			_page.optionsPage = this;
		}


		protected override IWin32Window Window {
			get {
				return _page;
			}
		}


		public override void LoadSettingsFromStorage()
		{
			_page.Initialize();
		}


		protected override void OnDeactivate(CancelEventArgs e)
		{
			base.OnDeactivate(e);

			if (!String.IsNullOrWhiteSpace(SyncConfiguration.Default.AzureKeyVaultName))
			{
				var repository = (AzureKeyVaultRepository)CoreContext.Current.GetService<IRepository>(nameof(SolutionSecrets.Core.Repository.RepositoryType.AzureKV));
				repository.RepositoryName = SyncConfiguration.Default.AzureKeyVaultName;
				if (repository.RepositoryName == null)
				{
					System.Windows.MessageBox.Show("The key vault URL is not correct.", Constants.MESSAGE_BOX_TITLE, MessageBoxButton.OK);
					e.Cancel = true;
				}
				else if (SyncConfiguration.Default.AzureKeyVaultName != repository.RepositoryName)
				{
					_page.SetDefaultKeyVaultName(repository.RepositoryName);
					e.Cancel = true;
				}
			}
		}


		private bool _saved = false;

		public override void SaveSettingsToStorage()
		{
			if (SyncConfiguration.Default.AzureKeyVaultName == String.Empty)
			{
				SyncConfiguration.Default.AzureKeyVaultName = null;
			}

			SyncConfiguration.Save();
			_saved = true;
		}


		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			if (!_saved)
			{
				SyncConfiguration.Refresh();
			}
			_saved = false;
		}

	}
}
