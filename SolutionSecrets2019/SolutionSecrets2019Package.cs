using System;
using System.Runtime.InteropServices;
using System.Threading;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using SolutionSecrets.Core.Encryption;
using SolutionSecrets.Core.Repository;
using SolutionSecrets2019.Commands;
using SolutionSecrets2019.Options;
using SolutionSecrets2019.Options.AzureKeyVault;
using SolutionSecrets2019.Options.General;
using SolutionSecrets2019.Options.GitHubGists;
using CoreContext = SolutionSecrets.Core.Context;
using Task = System.Threading.Tasks.Task;


namespace SolutionSecrets2019
{

	[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
	[InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
	[ProvideMenuResource("Menus.ctmenu", 1)]

	[Guid(PackageGuids.guidSolutionSecrets2019PkgString)]

	//<ProjectTypeGuids>{349c5851-65df-11da-9384-00065b846f21};{fae04ec0-301f-11d3-bf4b-00c04f79efbc}</ProjectTypeGuids>
	// https://docs.microsoft.com/en-us/visualstudio/extensibility/how-to-use-rule-based-ui-context-for-visual-studio-extensions?view=vs-2022

	[ProvideOptionPage(typeof(GeneralOptionPage), "Solution Secrets", "General", 0, 0, true)]
	[ProvideOptionPage(typeof(GitHubGistsOptionPage), "Solution Secrets", "GitHub Gists", 0, 0, true)]
	[ProvideOptionPage(typeof(AzureKeyVaultOptionPage), "Solution Secrets", "Azure Key Vault", 0, 0, true)]


	public sealed class SolutionSecrets2019Package : AsyncPackage
	{
		public static DTE2 _dte;

		protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
		{
			await base.InitializeAsync(cancellationToken, progress);

			await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
			_dte = await GetServiceAsync(typeof(DTE)) as DTE2;
			Assumes.Present(_dte);

			var cipher = new Cipher();
			await cipher.RefreshStatus();
			CoreContext.Current.AddService<ICipher>(cipher);

			var defaultRepository = new GistRepository();
			var azureKeyVaultRepository = new AzureKeyVaultRepository();

			CoreContext.Current.AddService<IRepository>(defaultRepository);
			CoreContext.Current.AddService<IRepository>(defaultRepository, nameof(SolutionSecrets.Core.Repository.RepositoryType.GitHub));
			CoreContext.Current.AddService<IRepository>(azureKeyVaultRepository, nameof(SolutionSecrets.Core.Repository.RepositoryType.AzureKV));

			await ConfigureCommand.InitializeAsync(this);
			await PullCommand.InitializeAsync(this);
			await PushCommand.InitializeAsync(this);
			await ClearCommand.InitializeAsync(this);
		}

	}
}