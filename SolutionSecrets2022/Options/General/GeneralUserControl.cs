using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SolutionSecrets.Core;

namespace SolutionSecrets2022.Options.General
{
	public partial class GeneralUserControl : UserControl
	{
		internal GeneralOptionPage optionsPage;

		public virtual void Initialize()
		{
			switch (SyncConfiguration.Default.Repository)
			{
				case SolutionSecrets.Core.Repository.RepositoryType.GitHub:
					cboxRepositoryType.SelectedIndex = 0;
					break;
				case SolutionSecrets.Core.Repository.RepositoryType.AzureKV:
					cboxRepositoryType.SelectedIndex = 1;
					break;
			}
		}


		public GeneralUserControl()
		{
			InitializeComponent();
		}

		private void cboxRepositoryType_SelectedIndexChanged(object sender, EventArgs e)
		{
			switch (cboxRepositoryType.SelectedIndex)
			{
				case 0:
					SyncConfiguration.Default.Repository = SolutionSecrets.Core.Repository.RepositoryType.GitHub;
					break;
				case 1:
					SyncConfiguration.Default.Repository = SolutionSecrets.Core.Repository.RepositoryType.AzureKV;
					break;
			}
		}
	}
}
