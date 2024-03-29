﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Security.KeyVault.Secrets;

namespace SolutionSecrets.Core.Repository
{
    public class AzureKeyVaultRepository : IRepository
    {

        private readonly IDictionary<string, string> _clouds = new Dictionary<string, string>
        {
            {".vault.azure.net",            "AzureCloud"},
            {".vault.azure.cn",             "AzureChinaCloud" },
            {".vault.usgovcloudapi.net",    "AzureUSGovernment" },
            {".vault.microsoftazure.de",    "AzureGermanCloud" }
        };

        private const string DEFAULT_CLOUD = ".vault.azure.net";
        private const string SECRET_PREFIX = "vs-secrets--";

        public bool EncryptOnClient => false;
        public string RepositoryType => "AzureKV";
        public string RepositoryTypeFullName => "Azure Key Vault";

        private Uri _repositoryUri;
        private string _repositoryName;

        public string RepositoryName
        {
            get => _repositoryName;
            set {
                if (value == null)
                {
                    _repositoryName = null;
                    _repositoryUri = null;
                }
                else
                {
                    string loweredValue = value.ToLowerInvariant();
                    if (Uri.TryCreate(loweredValue, UriKind.Absolute, out Uri repositoryUri) && repositoryUri != null)
                    {
                        _repositoryName = null;
                        _repositoryUri = null;

                        int vaultIndex = loweredValue.IndexOf(".vault.", StringComparison.Ordinal);
                        if (vaultIndex >= 0)
                        {
                            string cloudDomain = loweredValue.Substring(vaultIndex);
                            if (_clouds.ContainsKey(cloudDomain))
                            {
                                _repositoryName = repositoryUri.Scheme.Equals("https", StringComparison.Ordinal) ? loweredValue : null;
                                _repositoryUri = _repositoryName != null ? new Uri(_repositoryName) : null;
                            }
                        }
                    }
                    else
                    {
                        if (loweredValue.StartsWith("https://", StringComparison.Ordinal))
                        {
                            _repositoryName = loweredValue;
                        }
                        else
                        {
                            _repositoryName = $"https://{loweredValue}{DEFAULT_CLOUD}";
                        }
                        if (!Uri.TryCreate(_repositoryName, UriKind.Absolute, out _repositoryUri))
                        {
                            _repositoryName = null; 
                            _repositoryUri = null;
                        }
                    }
                }

                if (_client != null && _client.VaultUri != _repositoryUri)
                {
                    // If the vault URI has changed, we need to re-authorize the client.
                    _client = null;
                }
            }
        }


        private SecretClient _client;



        public string GetFriendlyName()
        {
            if (_repositoryName == null)
            {
                return null;
            }

            string name = _repositoryName;
            name = name.Substring(8);
            name = name.Substring(0, name.IndexOf(".vault.", StringComparison.Ordinal));

            string cloudDomain = _repositoryName;
            cloudDomain = cloudDomain.Substring(cloudDomain.IndexOf(".vault.", StringComparison.Ordinal));

            if (_clouds.TryGetValue(cloudDomain, out var cloud))
                return $"{name} ({cloud})";
            else
                return _repositoryName;
        }


        public async Task AuthorizeAsync()
        {
            if (_repositoryUri != null)
            {
                _client = new SecretClient(_repositoryUri);
                try
                {
                    var _ = await _client.GetSecretAsync("vs-secrets-fake");
                }
                catch (Azure.RequestFailedException ex)
                {
                    if (ex.Status == 401)
                    {
                        _client = null;
                        throw new UnauthorizedAccessException(ex.ErrorCode, ex);
                    }
                }
                catch (Azure.Identity.AuthenticationFailedException)
                {
                    _client = null;
                    throw;
                }
                catch
                { }
            }
        }


        public Task<bool> IsReady()
        {
            return Task.FromResult(_client != null);
        }


        public Task<ICollection<SolutionSettings>> PullAllSecretsAsync()
        {
            // No needs to implement this method.
            return Task.FromResult<ICollection<SolutionSettings>>(new List<SolutionSettings>());
        }



        private readonly string[] _secretNamePartsSeparator = new string[] { "--" };

        public async Task<ICollection<(string name, string content)>> PullFilesAsync(ISolution solution)
        {
            if (solution == null)
                throw new ArgumentNullException(nameof(solution));

            var files = new List<(string name, string content)>();

            if (_client == null)
            {
                return files;
            }

            string prefix = solution.Uid != Guid.Empty ?
                $"{SECRET_PREFIX}{solution.Uid}--" :
                $"{SECRET_PREFIX}{solution.Name}--";

            List<string> solutionSecretsName = new List<string>();
            foreach (var secretProperties in await _client.GetPropertiesOfSecrets())
            {
                if (secretProperties.Enabled == true && secretProperties.Name.StartsWith(prefix, StringComparison.Ordinal))
                {
                    solutionSecretsName.Add(secretProperties.Name);
                }
            }

            foreach (var secretName in solutionSecretsName)
            {
                var response = await _client.GetSecretAsync(secretName);
                var secret = response?.Value;
                if (secret == null)
                {
                    continue;
                }

                string[] nameParts = secretName.Split(_secretNamePartsSeparator, StringSplitOptions.RemoveEmptyEntries);
                if (nameParts.Length == 3)
                {
                    string fileNamePart = nameParts[2];
                    string fileName = fileNamePart == "secrets" ?
                            "secrets" :
                            $"secrets\\{fileNamePart}.json";
                    files.Add((name: fileName, content: secret.Value));
                }
            }

            return files;
        }


        public async Task<bool> PushFilesAsync(ISolution solution, ICollection<(string name, string content)> files)
        {
            if (solution == null)
                throw new ArgumentNullException(nameof(solution));

            if (files == null)
                throw new ArgumentNullException(nameof(files));

            if (_client == null)
            {
                return false;
            }

            foreach (var (name, content) in files)
            {
                string fileName = name;
                if (fileName.Contains('\\'))
                {
                    fileName = fileName.Substring((fileName.IndexOf('\\') + 1));
                    fileName = fileName.Substring(0, fileName.IndexOf('.'));
                }

                string secretName = solution.Uid != Guid.Empty ?
                    $"{SECRET_PREFIX}{solution.Uid}--{fileName}" :
                    $"{SECRET_PREFIX}{solution.Name}--{fileName}";

                int retry = 1;
                bool checkSecretEquality = true;
                while (true)
                {
                    if (checkSecretEquality)
                    {
                        try
                        {
                            // Read the current secret
                            var response = await _client.GetSecretAsync(secretName);
                            var secret = response?.Value;
                            if (secret?.Value == content)
                            {
                                break;
                            }
                        }
                        catch
                        { }
                    }

                    try
                    {
                        await _client.SetSecretAsync(secretName, content);
                    }
                    catch (Azure.RequestFailedException aex)
                    {
                        if (aex.Status == 409)
                        {
                            // Try to purge eventually deleted secret
                            try
                            {
                                await _client.PurgeDeletedSecretAsync(secretName);
                            }
                            catch (Azure.RequestFailedException aex2)
                            {
#pragma warning disable CA1508
                                if (aex2.Status == 403)
                                {
                                    Console.WriteLine($"\nERR: Cannot proceed with the operation.\n     Check if there is a secret named \"{secretName}\" that is deleted, but recoverable. In that case purge the secret or recover it before pushing local secrets.");
                                }
                                else
                                {
                                    Console.WriteLine($"\nERR: Cannot proceed with the operation.\n     There is a conflict with the secret named \"{secretName}\" that cannot be resolved. Contact the administrator of the Key Vault.");
                                }
#pragma warning restore CA1508
                                return false;
                            }
                            catch (Exception)
                            {
                                return false;
                            }

                            if (retry-- > 0)
                            {
                                checkSecretEquality = false;
                                continue;
                            }
                        }
                        return false;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"ERR: {ex.Message}");
                        return false;
                    }
                    break;
                }
            }

            return true;
        }


        public Task RefreshStatus()
        {
            return Task.CompletedTask;
        }


        public bool IsValid() => _repositoryName != null;


        public Task<string> StartDeviceFlowAuthorizationAsync()
        {
            return Task.FromResult<string>(null);
        }


        public Task CompleteDeviceFlowAuthorizationAsync()
        {
            return Task.CompletedTask;
        }


        public void AbortAuthorization()
        {
        }

    }
}
