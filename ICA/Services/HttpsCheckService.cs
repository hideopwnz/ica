using System.Net.Http;

namespace ICA.Services;

public class HttpsCheckService
{
    private static readonly HttpClient _client;
    private readonly string[] _sites;

    static HttpsCheckService()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        _client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(3)
        };
        _client.DefaultRequestHeaders.Add("User-Agent", "ICA/1.0");
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

    private static async Task<bool> CheckSiteAsync(string url)
    {
        try
        {
            var response = await _client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            return (int)response.StatusCode < 400;
        }
        catch
        {
            return false;
        }
    }
}
