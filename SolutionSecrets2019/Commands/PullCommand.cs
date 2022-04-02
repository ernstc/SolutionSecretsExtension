using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Windows;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;

using Newtonsoft.Json;

using SolutionSecrets.Core;

using Task = System.Threading.Tasks.Task;


namespace SolutionSecrets2019.Commands
{

	internal sealed class PullCommand : CommandAsync
	{

		public const int CommandId = PackageIds.cmdidPullSecrets;

		public static readonly Guid CommandSet = PackageGuids.guidSolutionSecrets2019CmdSet;


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
			string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
			PullSecretsAsync().Forget();
		}


		private async Task PullSecretsAsync()
		{
			var _cipher = Services.Cipher;
			var _repository = Services.Repository;

			if (!await _cipher.IsReady() || !await _repository.IsReady())
			{
				System.Windows.MessageBox.Show("You need to configure the solution secrets synchronization before using the Pull command.", Vsix.Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);

				var dialog = new ConfigDialog();
				dialog.ShowDialog(); 
				
				return;
			}

			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(); 
			var solutionFullName = SolutionSecrets2019Package._dte.Solution.FullName;

			SolutionFile solution = new SolutionFile(solutionFullName, _cipher);
			await UseStatusBarAsync($"Pulling secrets for solution: {solution.Name} ...");

			var configFiles = solution.GetProjectsSecretConfigFiles();
			if (configFiles.Count == 0)
			{
				await UseStatusBarAsync("No secrets found.");
				return;
			}

			_repository.SolutionName = solution.Name;

			var files = await _repository.PullFilesAsync();
			if (files.Count == 0)
			{
				await UseStatusBarAsync("No secrets found.");
				return;
			}

			// Validate header file
			HeaderFile header = null;
			foreach (var file in files)
			{
				if (file.name == "secrets" && file.content != null)
				{
					header = JsonConvert.DeserializeObject<HeaderFile>(file.content);
					break;
				}
			}

			if (header == null)
			{
				await UseStatusBarAsync("Error pulling secrets for the solution.");
				return;
			}

			Version headerVersion = new Version(header.visualStudioSolutionSecretsVersion);
			Version minVersion = new Version(Versions.MinimumFileFormatSupported);
			if (headerVersion.Major > minVersion.Major)
			{
				await UseStatusBarAsync("Secrets format not compatible.");
				return;
			}

			bool failed = false;
			foreach (var file in files)
			{
				if (file.name != "secrets")
				{
					if (file.content == null)
					{
						continue;
					}

					foreach (var configFile in configFiles)
					{
						if (configFile.UniqueFileName == file.name)
						{
							configFile.Content = file.content;
							if (configFile.Decrypt())
							{
								solution.SaveConfigFile(configFile);
							}
							else
							{
								failed = true;
							}
							break;
						}
					}

					if (failed)
					{
						break;
					}
				}
			}

			if (!failed)
				await UseStatusBarAsync("Secrets pulled successfully.");
			else
				await UseStatusBarAsync("Secrets pull has failed!");
		}

	}
}
