global using System;
global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using CoreContext = SolutionSecrets.Core.Context;
global using Task = System.Threading.Tasks.Task;

using System.Runtime.InteropServices;
using System.Threading;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using SolutionSecrets.Core;
using SolutionSecrets.Core.Encryption;
using SolutionSecrets.Core.Repository;
using SolutionSecrets2022.Options.AzureKeyVault;
using SolutionSecrets2022.Options.General;
using SolutionSecrets2022.Options.GitHubGists;


namespace SolutionSecrets2022
{
	[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
	[ProvideOptionPage(typeof(GeneralOptionPage), "Solution Secrets", "General", 0, 0, true)]
	[ProvideOptionPage(typeof(GitHubGistsOptionPage), "Solution Secrets", "GitHub Gists", 0, 0, true)]
	[ProvideOptionPage(typeof(AzureKeyVaultOptionPage), "Solution Secrets", "Azure Key Vault", 0, 0, true)]
	[ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.guidSolutionSecrets2022PkgString)]
	public sealed class SolutionSecrets2022Package : ToolkitPackage
    {
		public static DTE2 _dte;


		protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
			_dte = await GetServiceAsync(typeof(DTE)) as DTE2;
			Assumes.Present(_dte); 
			await this.RegisterCommandsAsync();

			var cipher = new Cipher();
			await cipher.RefreshStatus();
			Context.Current.AddService<ICipher>(cipher);

			var defaultRepository = new GistRepository();
			var azureKeyVaultRepository = new AzureKeyVaultRepository();

			CoreContext.Current.AddService<IRepository>(defaultRepository);
			CoreContext.Current.AddService<IRepository>(defaultRepository, nameof(SolutionSecrets.Core.Repository.RepositoryType.GitHub));
			CoreContext.Current.AddService<IRepository>(azureKeyVaultRepository, nameof(SolutionSecrets.Core.Repository.RepositoryType.AzureKV));
		}

	}
}