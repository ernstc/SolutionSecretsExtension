using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Windows;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;

using Newtonsoft.Json;

using SolutionSecrets.Core;

using Task = System.Threading.Tasks.Task;


namespace SolutionSecrets2019.Commands
{

	internal sealed class PushCommand : CommandAsync
	{

		public const int CommandId = PackageIds.cmdidPushSecrets;

		public static readonly Guid CommandSet = PackageGuids.guidSolutionSecrets2019CmdSet;


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
			: base(package)
		{
			commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

			var menuCommandID = new CommandID(CommandSet, CommandId);
			var menuItem = new MenuCommand(this.Execute, menuCommandID);
			commandService.AddCommand(menuItem);
		}


		private void Execute(object sender, EventArgs e)
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			PushSecretsAsync().Forget();
		}


		private async Task PushSecretsAsync()
		{
			var _cipher = Services.Cipher;
			var _repository = Services.Repository;

			if (!await _cipher.IsReady() || !await _repository.IsReady())
			{
				System.Windows.MessageBox.Show("You need to configure the solution secrets synchronization before using the Push command.", Vsix.Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);

				var dialog = new ConfigDialog();
				dialog.ShowDialog();

				return;
			}

			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			var solutionFullName = SolutionSecrets2019Package._dte.Solution.FullName;

			SolutionFile solution = new SolutionFile(solutionFullName, _cipher);
			await UseStatusBarAsync($"Pushing secrets for solution: {solution.Name} ...");

			var headerFile = new HeaderFile
			{
				visualStudioSolutionSecretsVersion = Vsix.Version,
				lastUpload = DateTime.UtcNow,
				solutionFile = solution.Name
			};

			List<(string fileName, string content)> files = new List<(string fileName, string content)>();
			files.Add(("secrets", JsonConvert.SerializeObject(headerFile, Formatting.None)));

			var configFiles = solution.GetProjectsSecretConfigFiles();
			if (configFiles.Count == 0)
			{
				return;
			}

			_repository.SolutionName = solution.Name;

			Dictionary<string, Dictionary<string, string>> secrets = new Dictionary<string, Dictionary<string, string>>();

			bool failed = false;
			foreach (var configFile in configFiles)
			{
				configFile.Load();
				if (configFile.Content != null)
				{
					if (configFile.Encrypt())
					{
						if (!secrets.ContainsKey(configFile.GroupName))
						{
							secrets.Add(configFile.GroupName, new Dictionary<string, string>());
						}
						secrets[configFile.GroupName].Add(configFile.FileName, configFile.Content);
					}
					else
					{
						failed = true;
						break;
					}
				}
			}

			foreach (var group in secrets)
			{
				string groupContent = JsonConvert.SerializeObject(group.Value);
				files.Add((group.Key, groupContent));
			}

			if (!failed)
			{
				if (await _repository.PushFilesAsync(files))
					await UseStatusBarAsync("Secrets pushed successfully.");
				else
					failed = true;
			}

			if (failed)
			{
				await UseStatusBarAsync("Secrets pushing has failed!");
			}
		}

	}
}
