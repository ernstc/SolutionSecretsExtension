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

	internal sealed class PushCommand : CommandAsync
	{

		public const int CommandId = PackageIds.cmdidPushSecrets;

		public static readonly Guid CommandSet = PackageGuids.guidSolutionSecrets2019CmdSet;

		private SolutionSecrets.Core.Commands.PushCommand _pushCommand;


		public static PushCommand Instance {
			get;
			private set;
		}


		public static async Task InitializeAsync(AsyncPackage package)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
			OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
			Instance = new PushCommand(package, commandService);
		}


		private PushCommand(AsyncPackage package, OleMenuCommandService commandService)
			: base(package, commandService)
		{
			commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

			var menuCommandID = new CommandID(CommandSet, CommandId);
			var menuItem = new MenuCommand(this.Execute, menuCommandID);
			commandService.AddCommand(menuItem);

			_pushCommand = new SolutionSecrets.Core.Commands.PushCommand(
				new VSEnvironment(package, commandService)
			);
		}


		private void Execute(object sender, EventArgs e)
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			PushSecretsAsync().Forget();
		}


		private async Task PushSecretsAsync()
		{
			await package.JoinableTaskFactory.SwitchToMainThreadAsync();
			var solutionFullName = SolutionSecrets2019Package._dte.Solution.FullName;

			await _pushCommand.Execute(solutionFullName);
		}

	}
}
