using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace xiv.raid.OAuth;

public class ApiLink
{
    private static ApiLink _instance;
    private static readonly object _lock = new();

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
    
    private readonly HttpClient _httpClient;
    private readonly Plugin _plugin;
    private string _fflogstoken;
    private DateTime _tokenExpiration;
    private Configuration configuration;

    private string ClientId;
    private string ClientSecret;
    private string TomestoneToken;
    private const string OAuthBaseUrl = "https://www.fflogs.com/oauth/token";
    private const string FFlogsApiBaseUrl = "https://www.fflogs.com/api/v2/client";
    private const string TomestoneApiBaseUrl = "https://tomestone.gg/api/character/profile/";

    private ApiLink(Plugin plugin)
    {
        _httpClient = new HttpClient();
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
            await RefreshAccessTokenAsync();
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _fflogstoken);
        return _httpClient;
    }

    public HttpClient GetAuthenticatedTomestoneHttpClient()
    {
        if (TomestoneToken == null)
        {
            throw new InvalidOperationException("Tomestone token has not been initialized.");
        }
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TomestoneToken);
        return _httpClient;
    }

    private async Task RefreshAccessTokenAsync()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, OAuthBaseUrl);
        
        var credentials = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{ClientId}:{ClientSecret}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        var formData = new MultipartFormDataContent
        {
            { new StringContent("client_credentials"), "grant_type" }
        };
        request.Content = formData;
        
        var response = await _httpClient.SendAsync(request);
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

    public async Task<String> GetFFLogsData(string query, object variable = null)
    {
        var client = await GetAuthenticatedFfLogsHttpClientAsync();

        var payload = new
        {
            query = query,
            variables = variable ?? new { }
        };

        var jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"{FFlogsApiBaseUrl}", content);
        
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Error calling FFLogs API: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
        }

        return await response.Content.ReadAsStringAsync();
    }

    public async Task<String> GetTomestoneData(string endpoint)
    {
        var client = GetAuthenticatedTomestoneHttpClient();
        var response = await client.GetAsync($"{TomestoneApiBaseUrl}/{endpoint}");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"Error fetching Tomestone data: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
        }
        
        return await response.Content.ReadAsStringAsync();
    }
    
}

class TokenResponse
{
    public string token_type { get; set; }
    public int expires_in { get; set; }
    public string access_token { get; set; }
}
