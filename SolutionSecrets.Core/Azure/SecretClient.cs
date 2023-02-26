using Azure.Core;
using Azure.Identity;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;


namespace Azure.Security.KeyVault.Secrets
{

    public class Response<T>
    {
        [JsonPropertyName("value")]
        public T Value { get; set; }

        [JsonPropertyName("error")]
        public ResponseError Error { get; set; }

        [JsonPropertyName("nextLink")]
        public string NextLink { get; set; }
    }



    public class ResponseError
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("innererror")]
        public ResponseError InnerError { get; set; }
    }



    public class KeyVaultSecretAttributes
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        [JsonPropertyName("created")]
        public int Created { get; set; }

        [JsonPropertyName("updated")]
        public int Updated { get; set; }

        [JsonPropertyName("recoveryLevel")]
        public string RecoveryLevel { get; set; }

        [JsonPropertyName("recoverableDays")]
        public int RecoverableDays { get; set; }
    }



    public class KeyVaultSecret
    {
        [JsonIgnore]
        public string Name { get; internal set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonIgnore]
        public bool Enabled => Attributes?.Enabled ?? false;

        [JsonPropertyName("attributes")]
        public KeyVaultSecretAttributes Attributes { get; set; }
    }



    public class SecretClient
    {
        private const string API_VERSION = "api-version=7.3";
        private const string _guidPattern = @"(?<guid>[\da-fA-F]{8}(-[\da-fA-F]{4}){3}-[\da-fA-F]{12})";

        private Uri _vaultUri;
        private string[] _tenants;
        private List<string> _includeTenants;
        private List<string> _excludeTenants;
        private string _tenant;
        private string _token;

        private static Dictionary<string, string> cacheVaultTenants = new Dictionary<string, string>();


        public SecretClient(Uri vaultUri)
        {
            if (vaultUri == null)
                throw new ArgumentNullException(nameof(vaultUri));

            _vaultUri = vaultUri;
        }


        public async Task<KeyVaultSecret[]> GetPropertiesOfSecrets()
        {
            var response = await SendAsync<Response<KeyVaultSecret[]>>(HttpMethod.Get, "/secrets");
            if (response != null)
            {
                string prefix = $"{_vaultUri}secrets/";
                foreach (var item in response.Value)
                {
                    item.Name = item.Id.Substring(prefix.Length);
                    int idx = item.Name.IndexOf('/');
                    if (idx >= 0)
                    {
                        item.Name = item.Name.Substring(0, idx);
                    }
                }
            }
            return response?.Value ?? new KeyVaultSecret[0];
        }


        public async Task<Response<KeyVaultSecret>> GetSecretAsync(string secretName, string version = null, CancellationToken cancellationToken = default)
        {
            var secret =  await SendAsync<KeyVaultSecret>(HttpMethod.Get, $"secrets/{secretName}");
            if (secret != null)
            {
                secret.Name = secretName;
                return new Response<KeyVaultSecret>
                {
                    Value = secret
                };
            }
            else
            {
                return null;
            }
        }


        public async Task<Response<KeyVaultSecret>> SetSecretAsync(string secretName, string content)
        {
            string payload = JsonSerializer.Serialize(new { value = content });
            var secret = await SendAsync<KeyVaultSecret>(HttpMethod.Put, $"secrets/{secretName}", payload);
            if (secret != null)
            {
                secret.Name = secretName;
                return new Response<KeyVaultSecret>
                {
                    Value = secret
                };
            }
            else
            {
                return null;
            }
        }


        public async Task PurgeDeletedSecretAsync(string secretName)
        {
            await SendAsync<KeyVaultSecret>(HttpMethod.Delete, $"secrets/{secretName}");
        }


        private async Task<T> SendAsync<T>(HttpMethod method, string path, string content = null)
            where T: class, new()
        {
            await EnsureTokenAsync();

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(method, $"{_vaultUri}{path}?{API_VERSION}");
                if (content != null)
                {
                    request.Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json");
                }
                var response = await httpClient.SendAsync(request);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    cacheVaultTenants[_vaultUri.Host] = _tenant;
                    return JsonSerializer.Deserialize<T>(responseBody);
                }

                var vaultResponse = JsonSerializer.Deserialize<Response<KeyVaultSecret>>(responseBody);
                if (vaultResponse.Error != null)
                {
                    if (vaultResponse.Error.Code == "Unauthorized")
                    {
                        if (vaultResponse.Error.Message.StartsWith("[TokenExpired]"))
                        {
                            InvalidateToken();
                            return await SendAsync<T>(method, path, content);
                        }
                        else if (vaultResponse.Error.Message.StartsWith("AKV10032"))
                        {
                            var matches = Regex.Matches(vaultResponse.Error.Message, _guidPattern);
                            if (matches.Count > 0)
                            {
                                _includeTenants.Clear();
                                for (int i = 0; i < matches.Count - 1; i++)
                                {
                                    _includeTenants.Add(matches[i].Value);
                                }
                                _excludeTenants.Clear();
                                _excludeTenants.Add(matches[matches.Count - 1].Value);
                                InvalidateToken();
                                return await SendAsync<T>(method, path, content);
                            }
                        }
                    }
                    else if (vaultResponse.Error.Code == "Forbidden")
                    {
                        if (vaultResponse.Error.InnerError?.Code == "SecretDisabled")
                        {
                            return null;
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return null;
            }
        }


        private async Task EnsureTokenAsync()
        {
            if (_token == null)
            {
                await GetTokenAsync();
            }
        }


        private void InvalidateToken()
        {
            _token = null;
        }


        private async Task GetTokenAsync()
        {
            if (cacheVaultTenants.ContainsKey(_vaultUri.Host))
                _tenant = cacheVaultTenants[_vaultUri.Host];
            else
            {
                if (_tenants == null)
                {
                    _tenants = await GetAccountTenantsAsync();
                    _includeTenants = new List<string>(_tenants);
                    _excludeTenants = new List<string>();
                }
                _tenant = SelectTenant(_tenants, _includeTenants, _excludeTenants);
            }

            var credential = GetCredential(_tenant);
            var context = new TokenRequestContext(new[] { "https://vault.azure.net/.default" });
            var token = await credential.GetTokenAsync(context, default);
            _token = token.Token;
        }


        private class TenantInfo
        {
            [JsonPropertyName("tenantId")]
            public string TenantId { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("tenantCategory")]
            public string TenantCategory { get; set; }

            [JsonPropertyName("defaultDomain")]
            public string DefaultDomain { get; set; }

            [JsonPropertyName("tenantType")]
            public string TenantType { get; set; }
        }


        private async static Task<string[]> GetAccountTenantsAsync()
        {
            string[] tenants = new string[0];

            var credential = GetCredential();
            var context = new TokenRequestContext(new[] { "https://management.azure.com/.default" });

            try
            {
                var token = await credential.GetTokenAsync(context, default);

                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Token);
                var response = await client.GetAsync("https://management.azure.com/tenants?api-version=2020-01-01");
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var tenantsResponse = System.Text.Json.JsonSerializer.Deserialize<Response<TenantInfo[]>>(json);
                    if (tenantsResponse.Value != null)
                    {
                        var list = new List<string>();
                        foreach (var item in tenantsResponse.Value)
                        {
                            list.Add(item.TenantId);
                        }
                        tenants = list.ToArray();
                    }
                }
            }
            catch (Azure.Identity.CredentialUnavailableException ex)
            {
                if (ex.InnerException != null && ex.InnerException is AggregateException aggregateEx)
                {
                    Exception lastEx = aggregateEx.InnerExceptions[aggregateEx.InnerExceptions.Count - 1];
                    if (lastEx.InnerException != null) lastEx = lastEx.InnerException;
                    string message = lastEx.Message;
                    while (Regex.IsMatch(message, @"TS\d{3}"))
                    {
                        message = message.Substring(message.IndexOf(":") + 1);
                    }
                    throw new Azure.Identity.AuthenticationFailedException(message.Trim(), ex);
                }
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            return tenants;
        }


        private static TokenCredential GetCredential(string tenant = null)
        {
            var options = new VisualStudioCredentialOptions();
            if (tenant != null)
            {
                options.TenantId = tenant;
            }

            return new VisualStudioCredential(options);
        }


        private static string SelectTenant(string[] tenants, IList<string> includeTenants = null, IList<string> excludeTenants = null)
        {
            List<string> t = new List<string>(tenants);

            if (includeTenants != null)
            {
                for (int i = t.Count - 1; i >= 0; i--)
                {
                    if (!includeTenants.Contains(t[i]))
                    {
                        t.Remove(t[i]);
                    }
                }
            }

            if (excludeTenants != null)
            {
                for (int i = 0; i < excludeTenants.Count; i++)
                {
                    t.Remove(excludeTenants[i]);
                }
            }

            return t.Count > 0 ? t[0] : null;
        }

    }
}
