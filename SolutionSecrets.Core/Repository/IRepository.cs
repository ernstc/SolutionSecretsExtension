﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionSecrets.Core.Repository
{

    public interface IRepository : IService
    {
        bool EncryptOnClient { get; }
        string RepositoryType { get; }
        string RepositoryTypeFullName { get; }
        string RepositoryName { get; set; }
        string GetFriendlyName();
        Task AuthorizeAsync();
        Task<bool> PushFilesAsync(ISolution solution, ICollection<(string name, string content)> files);
        Task<ICollection<(string name, string content)>> PullFilesAsync(ISolution solution);
        Task<ICollection<SolutionSettings>> PullAllSecretsAsync();
        bool IsValid();
        Task<string> StartDeviceFlowAuthorizationAsync();
        Task CompleteDeviceFlowAuthorizationAsync();
        void AbortAuthorization();
    }
}
