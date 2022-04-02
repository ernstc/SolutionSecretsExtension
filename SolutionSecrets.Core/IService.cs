using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionSecrets.Core
{
    public interface IService
    {
        Task<bool> IsReady();
        Task RefreshStatus();
    }
}
