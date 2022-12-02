using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;
using SolutionSecrets.Core;

namespace SolutionSecrets2019.Options.General
{
    internal class GeneralOptionPage : DialogPage
	{

		private GeneralUserControl _page;


		public GeneralOptionPage()
		{
			_page = new GeneralUserControl();
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
			SyncConfiguration.Save();
		}
	}
}
