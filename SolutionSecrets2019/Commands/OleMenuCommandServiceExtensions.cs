using System;
using System.ComponentModel.Design;
using SolutionSecrets.Core;

namespace Microsoft.VisualStudio.Shell
{
	public static class OleMenuCommandServiceExtensions
    {
        public static void OpenOption<TPage>(this OleMenuCommandService commandService) where TPage : DialogPage
		{
			SyncConfiguration.Refresh();
			Guid cmdGroup = typeof(VSConstants.VSStd97CmdID).GUID;
			var cmd = new CommandID(cmdGroup, VSConstants.cmdidToolsOptions);
			commandService.GlobalInvoke(cmd, typeof(TPage).GUID.ToString());
		}
    }
}
