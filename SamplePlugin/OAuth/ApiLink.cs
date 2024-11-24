using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace xiv.raid.OAuth;

public class ApiLink
{
    private static ApiLink _instance;
    private static readonly object _lock = new();
    
    private static readonly SemaphoreSlim _tokenRefreshSemaphore = new(1, 1);
    private static readonly SemaphoreSlim _fflogsDataFetching = new(1, 1);
    private static readonly SemaphoreSlim _tomestoneDataFetching = new(1, 1);

    public static ApiLink Instance
    {
        get
        {
            if (_instance == null)
            {
                throw new InvalidOperationException("ApiLink has not been initialized.");
            }
            return _instance;
        }
    }
    
    private readonly HttpClient _httpClientFfLogs;
    private readonly HttpClient _httpClientTomestone;
    private readonly Plugin _plugin;
    private string _fflogstoken;
    private DateTime _tokenExpiration;
    private Configuration configuration;

    private string ClientId;
    private string ClientSecret;
    private string TomestoneToken;
    private const string OAuthBaseUrl = "https://www.fflogs.com/oauth/token";
    private const string FFlogsApiBaseUrl = "https://www.fflogs.com/api/v2/client";
    private const string TomestoneApiBaseUrl = "https://tomestone.gg/api/character/profile";

    private ApiLink(Plugin plugin)
    {
        _httpClientFfLogs = new HttpClient();
        _httpClientTomestone = new HttpClient();
        _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
        configuration = plugin.Configuration;
        
        ClientId = configuration.ClientId;
        ClientSecret = configuration.SecretId;
        TomestoneToken = configuration.TomestoneToken;
    }

    public static void Initialize(Plugin plugin)
    {
        if (_instance != null)
        {
            throw new InvalidOperationException("ApiLink has already been initialized.");
        }

        lock (_lock)
        {
            if (_instance == null)
            {
                _instance = new ApiLink(plugin);
            }
        }
    }

    public void RefreshConfig()
    {
        ClientId = configuration.ClientId;
        ClientSecret = configuration.SecretId;
        TomestoneToken = configuration.TomestoneToken;
    }

    public async Task<HttpClient> GetAuthenticatedFfLogsHttpClientAsync()
    {
        if (string.IsNullOrEmpty(_fflogstoken) || DateTime.UtcNow >= _tokenExpiration)
        {
            DalamudApi.PluginLog.Debug("Refreshing fflogs token");
            await RefreshAccessTokenAsync();
            _httpClientFfLogs.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _fflogstoken);
        }
        
        DalamudApi.PluginLog.Debug("Granting client with token : "+_httpClientFfLogs.DefaultRequestHeaders.Authorization);
        return _httpClientFfLogs;
    }

    public HttpClient GetAuthenticatedTomestoneHttpClient()
    {
        if (TomestoneToken == null)
        {
            throw new InvalidOperationException("Tomestone token has not been initialized.");
        }
        _httpClientTomestone.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TomestoneToken);
        return _httpClientTomestone;
    }

    private async Task RefreshAccessTokenAsync()
    {
        // Use the semaphore to ensure only one thread can refresh the token at a time
        await _tokenRefreshSemaphore.WaitAsync();
        try
        {
            // If token is still valid, no need to refresh
            if (!string.IsNullOrEmpty(_fflogstoken) && DateTime.UtcNow < _tokenExpiration)
            {
                return; // Token is still valid, no need to refresh
            }

            // Proceed with refreshing the token
            using var request = new HttpRequestMessage(HttpMethod.Post, OAuthBaseUrl);
            
            var credentials = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{ClientId}:{ClientSecret}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            var formData = new MultipartFormDataContent
            {
                { new StringContent("client_credentials"), "grant_type" }
            };
            request.Content = formData;
            
            var response = await _httpClientFfLogs.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error fetching access token: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            }

            var content = await response.Content.ReadAsStringAsync();
            
            var tokenResponse = System.Text.Json.JsonSerializer.Deserialize<TokenResponse>(content);
            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.access_token))
            {
                throw new Exception("Invalid token response from FFLogs API.");
            }

            _fflogstoken = tokenResponse.access_token;
            _tokenExpiration = DateTime.UtcNow.AddSeconds(tokenResponse.expires_in - 60);
        }
        finally
        {
            _tokenRefreshSemaphore.Release(); // Ensure semaphore is always released
        }
    }

    public async Task<String> GetFFLogsData(string query, object variable = null)
    {
        await _fflogsDataFetching.WaitAsync();

        try
        {
            var client = await GetAuthenticatedFfLogsHttpClientAsync();

            var payload = new
            {
                query = query,
                variables = variable ?? new { }
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{FFlogsApiBaseUrl}", content);

            if (!response.IsSuccessStatusCode)
            {
                DalamudApi.PluginLog.Error("Error getting fflogs data : " + response);
                throw new Exception(
                    $"Error calling FFLogs API: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            }

            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        } finally
        {
            _fflogsDataFetching.Release();
        }
    }

    public async Task<String> GetTomestoneData(string server, string name)
    {
        await _tomestoneDataFetching.WaitAsync();

        try
        {
            var client = GetAuthenticatedTomestoneHttpClient();
            DalamudApi.PluginLog.Debug("Sending request to : " + $"{TomestoneApiBaseUrl}/{server}/{name.ToLower()}");
            var response = await client.GetAsync($"{TomestoneApiBaseUrl}/{server}/{name.ToLower()}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Error fetching Tomestone data: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            }

            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        } finally
        {
            _tomestoneDataFetching.Release();
        }
    }
    
}

class TokenResponse
{
    public string token_type { get; set; }
    public int expires_in { get; set; }
    public string access_token { get; set; }
}
