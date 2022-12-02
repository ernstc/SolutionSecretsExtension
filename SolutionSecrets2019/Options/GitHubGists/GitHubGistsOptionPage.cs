using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;

namespace SolutionSecrets2019.Options.GitHubGists
{
    internal class GitHubGistsOptionPage : DialogPage
	{

		private GitHubGistsUserControl _page;


		public GitHubGistsOptionPage()
		{
			_page = new GitHubGistsUserControl();
			_page.optionsPage = this;
		}


		protected override IWin32Window Window {
			get {
				return _page;
			}
		}


		public override async void LoadSettingsFromStorage()
		{
			await Services.Cipher.RefreshStatus();
			await Services.Repository.RefreshStatus();
			await _page.InitializeAsync();
		}

	}
}
