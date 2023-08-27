using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Windows;
using Azure.Identity;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;

using Newtonsoft.Json;

using SolutionSecrets.Core;
using SolutionSecrets.Core.Repository;
using Task = System.Threading.Tasks.Task;


namespace SolutionSecrets2019.Commands
{

	internal sealed class PullCommand : CommandAsync
	{

		public const int CommandId = PackageIds.cmdidPullSecrets;

		public static readonly Guid CommandSet = PackageGuids.guidSolutionSecrets2019CmdSet;

		private SolutionSecrets.Core.Commands.PullCommand _pullCommand;


		public static PullCommand Instance {
			get;
			private set;
		}


		public static async Task InitializeAsync(AsyncPackage package)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
			OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
			Instance = new PullCommand(package, commandService);
		}


		private PullCommand(AsyncPackage package, OleMenuCommandService commandService)
			: base(package, commandService)
		{
			commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

			var menuCommandID = new CommandID(CommandSet, CommandId);
			var menuItem = new MenuCommand(this.Execute, menuCommandID);
			commandService.AddCommand(menuItem);

			_pullCommand = new SolutionSecrets.Core.Commands.PullCommand(
				new VSEnvironment(package, commandService)
			);
		}


		private void Execute(object sender, EventArgs e)
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			PullSecretsAsync().Forget();
		}


		private async Task PullSecretsAsync()
		{
			await package.JoinableTaskFactory.SwitchToMainThreadAsync();
			var solutionFullName = SolutionSecrets2019Package._dte.Solution.FullName;

			await _pullCommand.Execute(solutionFullName);
		}

	}
}
