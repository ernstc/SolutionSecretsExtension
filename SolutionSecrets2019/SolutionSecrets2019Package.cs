using System;
using System.Runtime.InteropServices;
using System.Threading;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using SolutionSecrets2019.Commands;
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

	public sealed class SolutionSecrets2019Package : AsyncPackage
	{
		public static DTE2 _dte;


		protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
		{
			await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
			_dte = await GetServiceAsync(typeof(DTE)) as DTE2;
			Assumes.Present(_dte);

			await ConfigureCommand.InitializeAsync(this);
			await PullCommand.InitializeAsync(this);
			await PushCommand.InitializeAsync(this);
			await DeleteLocallyCommand.InitializeAsync(this);
		}
	}
}