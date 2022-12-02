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

namespace SolutionSecrets2019.Options.AzureKeyVault
{
	public partial class AzureKeyVaultUserControl : UserControl
	{
		internal AzureKeyVaultOptionPage optionsPage;

		public AzureKeyVaultUserControl()
		{
			InitializeComponent();
		}


		public void Initialize()
		{
			txtDefaultKeyVaultUrl.Text = SyncConfiguration.Default.AzureKeyVaultName;
		}


		private void txtDefaultKeyVaultUrl_TextChanged(object sender, EventArgs e)
		{
			SyncConfiguration.Default.AzureKeyVaultName = txtDefaultKeyVaultUrl.Text;
		}
	}
}
