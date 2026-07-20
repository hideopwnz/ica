using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;

namespace ICA.Services;

public class UpdateService
{
    private const string RepoOwner = "hideopwnz";
    private const string RepoName = "ica";
    private static readonly HttpClient _client;

    static UpdateService()
    {
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Add("User-Agent", "ICA-Updater");
    }

    public async Task<bool> CheckAndUpdateAsync()
    {
        try
        {
            var url = $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest";
            var json = await _client.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);

            var tag = doc.RootElement.GetProperty("tag_name").GetString();
            if (tag == null) return false;

            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version!;
            var remoteVersion = Version.Parse(tag.TrimStart('v', 'V'));

            if (remoteVersion <= currentVersion) return false;

            var assets = doc.RootElement.GetProperty("assets");
            string? downloadUrl = null;

            foreach (var asset in assets.EnumerateArray())
            {
                var name = asset.GetProperty("name").GetString();
                if (name != null && name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    downloadUrl = asset.GetProperty("browser_download_url").GetString();
                    break;
                }
            }

            if (downloadUrl == null) return false;

            await DownloadAndReplaceAsync(downloadUrl);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static async Task DownloadAndReplaceAsync(string url)
    {
        var currentExe = Environment.ProcessPath!;
        var dir = Path.GetDirectoryName(currentExe)!;
        var tempExe = Path.Combine(dir, "ICA_update.exe");
        var batPath = Path.Combine(dir, "update.bat");

        var data = await _client.GetByteArrayAsync(url);
        await File.WriteAllBytesAsync(tempExe, data);

        var processName = Path.GetFileNameWithoutExtension(currentExe);
        var bat = "@echo off\r\n" +
                  "timeout /t 2 /nobreak >nul\r\n" +
                  $"taskkill /f /im \"{processName}.exe\" >nul 2>&1\r\n" +
                  "timeout /t 1 /nobreak >nul\r\n" +
                  $"move /y \"{tempExe}\" \"{currentExe}\" >nul\r\n" +
                  $"start \"\" \"{currentExe}\"\r\n" +
                  "del \"%~f0\"\r\n";

        await File.WriteAllTextAsync(batPath, bat);

        Process.Start(new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c \"{batPath}\"",
            CreateNoWindow = true,
            UseShellExecute = false
        });

        Environment.Exit(0);
    }
}
