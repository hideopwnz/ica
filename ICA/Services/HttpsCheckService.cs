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
            AllowAutoRedirect = false,
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
        // Ранний выход: первый ответивший сайт = интернет есть.
        // Остальные не опрашиваются -> трафик в 3 раза меньше.
        foreach (var site in _sites)
        {
            if (await CheckSiteAsync(site))
                return true;
        }
        return false;
    }

    private async Task<bool> CheckSiteAsync(string site)
    {
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Head, site);
            using var resp = await _client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
            // Любой HTTP-ответ доказывает связность (даже 405 на HEAD).
            return true;
        }
        catch
        {
            return false;
        }
    }
}