using System;
using System.Threading.Tasks;

namespace SolutionSecrets.Core
{
    public interface IService
    {
        Task<bool> IsReady();
        Task RefreshStatus();
    }
}

