using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionSecrets.Core.Repository
{
    public interface IRepository : IService
    {
        string SolutionName { get; set; }
        Task<string> StartDeviceFlowAuthorizationAsync();
        Task CompleteDeviceFlowAuthorizationAsync();
        Task<bool> PushFilesAsync(ICollection<(string name, string content)> files);
        Task<ICollection<(string name, string content)>> PullFilesAsync();
    }
}
