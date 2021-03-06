using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace SolutionSecrets.Core.Repository
{

    public class GistRepository : IRepository
    {

        private const string APP_DATA_FILENAME = "github.json";

        private const string CLIENT_ID = "b0e87a43f71306d87649";
        private const string SCOPE = "gist";

        private DeviceFlowResponse _deviceFlowResponse;

        private string _oauthAccessToken;
        private string _repositoryName;
        private Gist _gist;


        public string SolutionName
        {
            get
            {
                return _repositoryName;
            }
            set
            {
                _repositoryName = value;
                _gist = null;
            }
        }



        class DeviceFlowResponse
        {
            public string device_code { get; set; }
            public string user_code { get; set; }
            public string verification_uri { get; set; }
            public int expires_in { get; set; }
            public int interval { get; set; }
        }



        class AccessTokenResponse
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public string scope { get; set; }
            public string error { get; set; }
            public string error_description { get; set; }
            public string error_uri { get; set; }
        }



        class GistFile
        {
            public string content { get; set; }
            public string raw_url { get; set; }
        }



        class Gist
        {
            public string id { get; set; }
            public string description { get; set; }
            public bool @public { get; set; }
            public Dictionary<string, GistFile> files { get; set; }
        }



        class RepositoryAppData
        {
            public string access_token { get; set; }
        }



        public GistRepository()
        {
            RefreshStatus();
        }


        public async Task<bool> IsReady()
        {
            await CheckAccessToken();
            return _oauthAccessToken != null;
        }


        public Task RefreshStatus()
        {
            var repositoryData = AppData.LoadData<RepositoryAppData>(APP_DATA_FILENAME);
            _oauthAccessToken = repositoryData?.access_token;
            return Task.CompletedTask;
        }


        public async Task<string> StartDeviceFlowAuthorizationAsync()
        {
            _deviceFlowResponse = await SendRequest<DeviceFlowResponse>(HttpMethod.Post, $"https://github.com/login/device/code?client_id={CLIENT_ID}&scope={SCOPE}");
            return _deviceFlowResponse?.user_code;
        }


        public async Task CompleteDeviceFlowAuthorizationAsync()
        {
            if (_deviceFlowResponse == null)
            {
                return;
            }

            await CheckAccessToken();

            if (_oauthAccessToken == null)
            {
                OpenBrowser(_deviceFlowResponse.verification_uri);

                for (int seconds = _deviceFlowResponse.expires_in; seconds > 0; seconds -= _deviceFlowResponse.interval)
                {
                    var accessTokenResponse = await SendRequest<AccessTokenResponse>(
                        HttpMethod.Post,
                        $"https://github.com/login/oauth/access_token?client_id={CLIENT_ID}&device_code={_deviceFlowResponse.device_code}&grant_type=urn:ietf:params:oauth:grant-type:device_code"
                        );

                    if (accessTokenResponse?.access_token != null)
                    {
                        _oauthAccessToken = accessTokenResponse.access_token;

                        AppData.SaveData(APP_DATA_FILENAME, new RepositoryAppData
                        {
                            access_token = _oauthAccessToken
                        });

                        break;
                    }

                    await Task.Delay(1000 * _deviceFlowResponse.interval);
                }
            }
        }


        private async Task<Gist> GetGistAsync()
        {
            if (_gist == null && _repositoryName != null)
            {
                const int GIST_PER_PAGE = 100;
                for (int page = 1; page < 1000; page++)
                {
                    var gists = await SendRequest<List<Gist>>(HttpMethod.Get, $"https://api.github.com/gists?per_page={GIST_PER_PAGE}&page={page}");
                    if (gists == null || gists.Count == 0)
                    {
                        break;
                    }
                    for (int i = 0; i < gists.Count; i++)
                    {
                        var gist = gists[i];
                        if (gist.description == _repositoryName)
                        {
                            return _gist = gist;
                        }
                    }
                    if (gists.Count < GIST_PER_PAGE)
                    {
                        break;
                    }
                }
            }
            return _gist;
        }


        public async Task<ICollection<(string name, string content)>> PullFilesAsync()
        {
            List<(string name, string content)> files = new List<(string name, string content)>();
            var gist = await GetGistAsync();
            if (gist?.files != null)
            {
                foreach (var file in gist.files)
                {
                    string content = file.Value.content;
                    if (content == null && file.Value.raw_url != null)
                    {
                        HttpClient httpClient = new HttpClient();
                        try
                        {
                            content = await httpClient.GetStringAsync(file.Value.raw_url);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    files.Add((file.Key, content));
                }
            }
            return files;
        }


        public async Task<bool> PushFilesAsync(ICollection<(string name, string content)> files)
        {
            var gist = await GetGistAsync();

            HttpClient client = new HttpClient(new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            });

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _oauthAccessToken);

            if (client.DefaultRequestHeaders.UserAgent.TryParseAdd(Constants.USER_AGENT))
                client.DefaultRequestHeaders.Add("User-Agent", Constants.USER_AGENT);

            HttpRequestMessage request = gist == null ?
                new HttpRequestMessage(HttpMethod.Post, "https://api.github.com/gists") :
                new HttpRequestMessage(new HttpMethod("PATCH"), String.Format("https://api.github.com/gists/{0}", gist.id));

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));

            var payload = new Gist
            {
                description = _repositoryName,
                @public = false,
                files = new Dictionary<string, GistFile>()
            };

            foreach (var file in files)
            {
                payload.files.Add(file.name, new GistFile
                {
                    content = file.content
                });
            }

            try
            {
#if NET5_0_OR_GREATER
                string payloadJson = JsonSerializer.Serialize(payload, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
#else
                string payloadJson = JsonConvert.SerializeObject(payload, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
#endif
                request.Content = new StringContent(payloadJson, Encoding.UTF8, "application/json");

                var response = await client.SendAsync(request);
                var responseJson = await response.Content.ReadAsStringAsync();
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }


        private async Task CheckAccessToken()
        {
            if (_oauthAccessToken == null)
            {
                return;
            }

            HttpClient client = new HttpClient(new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            });

            if (_oauthAccessToken != null)
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _oauthAccessToken);

            if (client.DefaultRequestHeaders.UserAgent.TryParseAdd(Constants.USER_AGENT))
                client.DefaultRequestHeaders.Add("User-Agent", Constants.USER_AGENT);

            var message = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/gists/00000000000000000000000000000000");
            message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            try
            {
                var response = await client.SendAsync(message).ConfigureAwait(false);
                if (response.StatusCode != HttpStatusCode.NotFound)
                {
                    _oauthAccessToken = null;
                    AppData.SaveData(APP_DATA_FILENAME, new RepositoryAppData
                    {
                        access_token = null
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERR: {ex.Message}");
            }
        }


        private async Task<T> SendRequest<T>(HttpMethod method, string uri)
            where T : class, new()
        {
            HttpClient client = new HttpClient(new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            });

            if (_oauthAccessToken != null)
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _oauthAccessToken);

            if (client.DefaultRequestHeaders.UserAgent.TryParseAdd(Constants.USER_AGENT))
                client.DefaultRequestHeaders.Add("User-Agent", Constants.USER_AGENT);

            var message = new HttpRequestMessage(method, uri);
            message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            try
            {
                var response = await client.SendAsync(message).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var data = JsonConvert.DeserializeObject<T>(jsonContent);
                    return data;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERR: {ex.Message}");
            }
            return null;
        }


        private static void OpenBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

    }
}
