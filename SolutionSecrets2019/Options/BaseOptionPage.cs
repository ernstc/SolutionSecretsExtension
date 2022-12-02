using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;

namespace SolutionSecrets2019.Options
{
	/// <summary>
	/// A base class for a DialogPage to show in Tools -> Options.
	/// </summary>
	internal class BaseOptionPage<TModel> : DialogPage
		where TModel : BaseOptionModel<TModel>, new()
	{
		private BaseOptionModel<TModel> _model;

		public BaseOptionPage()
		{
#pragma warning disable VSTHRD104 // Offer async methods
			_model = ThreadHelper.JoinableTaskFactory.Run(BaseOptionModel<TModel>.CreateAsync);
#pragma warning restore VSTHRD104 // Offer async methods
		}

		public override object AutomationObject => _model;

		public override void LoadSettingsFromStorage()
		{
			_model.Load();
		}

		public override void SaveSettingsToStorage()
		{
			_model.Save();
		}
	}
}
