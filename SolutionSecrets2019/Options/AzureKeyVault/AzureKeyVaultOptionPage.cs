using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;
using SolutionSecrets.Core;

namespace SolutionSecrets2019.Options.AzureKeyVault
{
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
			SyncConfiguration.Refresh();
			_page.Initialize();
		}


		public override void SaveSettingsToStorage()
		{
			if (SyncConfiguration.Default.AzureKeyVaultName == String.Empty)
			{
				SyncConfiguration.Default.AzureKeyVaultName = null;
			}
			SyncConfiguration.Save();
		}
	}
}
