using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SolutionSecrets.Core
{
    public interface IEnvironment
    {
        Task OpenOptionPageAsync(OptionPage optionPage);
        Task ShowDialogMessageAsync(string message);
        Task ShowStatusMessageAsync(string message);
    }
}