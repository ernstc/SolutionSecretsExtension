using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionSecrets.Core.IO
{
    public interface IConsoleInput
    {
        ConsoleKeyInfo ReadKey();
    }
}
