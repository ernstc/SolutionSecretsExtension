using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionSecrets.Core.IO
{
    public interface IFileSystem
    {
        string GetApplicationDataFolderPath();
        string GetCurrentDirectory();
        string GetSecretsFolderPath();
    }
}
