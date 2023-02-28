using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;
using SolutionSecrets.Core.Encryption;
using SolutionSecrets.Core.Repository;


namespace SolutionSecrets2022.Options.GitHubGists
{
	[ComVisible(true)]
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
			var cipher = CoreContext.Current.GetService<ICipher>();
			var repository = CoreContext.Current.GetService<IRepository>(nameof(RepositoryType.GitHub));

			await cipher.RefreshStatus();
			await repository.RefreshStatus();
			await _page.InitializeAsync();
		}

	}
}
