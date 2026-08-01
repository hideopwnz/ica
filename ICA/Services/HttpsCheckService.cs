using System.Net.Http;

namespace ICA.Services;

public class HttpsCheckService
{
    private readonly string[] _sites;
    private static readonly HttpClient _client;

    static HttpsCheckService()
    {
        var handler = new HttpClientHandler
        {
            UseProxy = false,
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        _client = new HttpClient(handler);
        _client.Timeout = TimeSpan.FromSeconds(5);
    }

    public HttpsCheckService(string[] sites)
    {
        _sites = sites;
    }

    public async Task<bool> IsInternetAvailableAsync()
    {
        var tasks = _sites.Select(site => CheckSiteAsync(site)).ToArray();
        var results = await Task.WhenAll(tasks);
        return results.Any(r => r);
    }

    private async Task<bool> CheckSiteAsync(string site)
    {
        try
        {
            var response = await _client.GetAsync(site);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}

