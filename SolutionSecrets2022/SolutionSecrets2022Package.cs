global using System;
global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using Task = System.Threading.Tasks.Task;

using System.Runtime.InteropServices;
using System.Threading;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using SolutionSecrets.Core;
using SolutionSecrets.Core.Encryption;
using SolutionSecrets.Core.Repository;

namespace SolutionSecrets2022
{
	[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
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
			Context.Current.AddService<IRepository>(defaultRepository);
			Context.Current.AddService<IRepository>(defaultRepository, nameof(RepositoryTypesEnum.GitHub));
			Context.Current.AddService<IRepository>(new AzureKeyVaultRepository(), nameof(RepositoryTypesEnum.AzureKV));
		}
	}
}