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

	//[ProvideCodeGenerator(typeof(VsctGenerator), VsctGenerator.Name, VsctGenerator.Description, true, ProjectSystem = ProvideCodeGeneratorAttribute.CSharpProjectGuid, RegisterCodeBase = true)]
	//[ProvideCodeGeneratorExtension(VsctGenerator.Name, ".vsct")]

	//[ProvideCodeGenerator(typeof(VsixManifestGenerator), VsixManifestGenerator.Name, VsixManifestGenerator.Description, true, ProjectSystem = ProvideCodeGeneratorAttribute.CSharpProjectGuid, RegisterCodeBase = true)]
	//[ProvideCodeGeneratorExtension(VsixManifestGenerator.Name, ".vsixmanifest")]

	[Guid(PackageGuids.guidSolutionSecrets2019PkgString)]


	//[ProvideAutoLoad(Microsoft.VisualStudio.Shell.Interop.UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
	//[ProvideUIContextRule(PackageGuids.guidUIContextString,
	//	name: "UI Context",
	//	expression: "vsct | vsixmanifest | txt",
	//	termNames: new[] { "vsct", "vsixmanifest", "txt" },
	//	termValues: new[] { "HierSingleSelectionName:.vsct$", "HierSingleSelectionName:.vsixmanifest$", "HierSingleSelectionName:.txt$" })]

	//[ProvideUIContextRule(PackageGuids.guidUIContextString,
	//	name: "UI Context",
	//	expression: "SingleProject | MultipleProjects",
	//	termNames: new[] { "AspNet", "DotNetCore", "SingleProject", "MultipleProjects" },
	//	termValues: new[] { "ActiveProjectFlavor:349c5851-65df-11da-9384-00065b846f21", "ActiveProjectCapability:DotNetCore", VSConstants.UICONTEXT.SolutionHasSingleProject_string, VSConstants.UICONTEXT.SolutionHasMultipleProjects_string })]


	//<ProjectTypeGuids>{349c5851-65df-11da-9384-00065b846f21};{fae04ec0-301f-11d3-bf4b-00c04f79efbc}</ProjectTypeGuids>
	// https://docs.microsoft.com/en-us/visualstudio/extensibility/how-to-use-rule-based-ui-context-for-visual-studio-extensions?view=vs-2022



	[ProvideOptionPage(typeof(GeneralOptionPage), "Solution Secrets", "General", 0, 0, true)]
	[ProvideOptionPage(typeof(GitHubGistsOptionPage), "Solution Secrets", "GitHub Gists", 0, 0, true)]
	[ProvideOptionPage(typeof(AzureKeyVaultOptionPage), "Solution Secrets", "Azure Key Vault", 0, 0, true)]



	//[ProvideOptionPage(typeof(GitHubOptions), "Solution Secrects", "GitHub", 0, 0, true)]
	//[ProvideOptionPage(typeof(AzureKeyVaultOptions), "Solution Secrects", "Azure Key Vault", 0, 0, true)]



	public sealed class SolutionSecrets2019Package : AsyncPackage
	{
		public static DTE2 _dte;

		//public static GlobalOptions GlobalOptions {
		//	get;
		//	private set;
		//}

		//public static GitHubOptions GitHubOptions {
		//	get;
		//	private set;
		//}

		//public static AzureKeyVaultOptions AzureKeyVaultOptions {
		//	get;
		//	private set;
		//}


		protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
		{
			await base.InitializeAsync(cancellationToken, progress);

			await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
			_dte = await GetServiceAsync(typeof(DTE)) as DTE2;
			Assumes.Present(_dte);


			//GlobalOptions = (GlobalOptions)GetDialogPage(typeof(GlobalOptions));
			//GitHubOptions = (GitHubOptions)GetDialogPage(typeof(GitHubOptions));
			//AzureKeyVaultOptions = (AzureKeyVaultOptions)GetDialogPage(typeof(AzureKeyVaultOptions));
			try
			{
				var cipher = new Cipher();
				await cipher.RefreshStatus();
				CoreContext.Current.AddService<ICipher>(cipher);

				var defaultRepository = new GistRepository();
				CoreContext.Current.AddService<IRepository>(defaultRepository);
				CoreContext.Current.AddService<IRepository>(defaultRepository, nameof(SolutionSecrets.Core.Repository.RepositoryType.GitHub));
				CoreContext.Current.AddService<IRepository>(new AzureKeyVaultRepository(), nameof(SolutionSecrets.Core.Repository.RepositoryType.AzureKV));


				await ConfigureCommand.InitializeAsync(this);
				await PullCommand.InitializeAsync(this);
				await PushCommand.InitializeAsync(this);
				await ClearCommand.InitializeAsync(this);
			}
			catch (Exception)
			{
				throw;
			}
		}

	}
}