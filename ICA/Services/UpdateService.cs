using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using ICA.Views;

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
        UpdateWindow? window = null;

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

            // Показываем окно обновления
            window = new UpdateWindow();
            window.SetStatus(
                $"Текущая версия: {currentVersion.ToString(3)}\n" +
                $"Доступна новая версия: {remoteVersion.ToString(3)}\n" +
                $"Скачивание обновления...");
            window.Show();

            var currentExe = Environment.ProcessPath!;
            var dir = Path.GetDirectoryName(currentExe)!;
            var tempExe = Path.Combine(dir, "ICA_update.exe");

            await DownloadWithProgressAsync(downloadUrl, tempExe, window);

            // Успех
            window.SetStatus(
                $"Обновление успешно завершено!\n" +
                $"Программа перезапустится в версию {remoteVersion.ToString(3)}...");
            window.SetProgress(100);

            await Task.Delay(2000);

            ApplyUpdate(tempExe, currentExe);
            return true;
        }
        catch (Exception ex)
        {
            if (window != null)
            {
                window.SetStatus(
                    $"Ошибка обновления:\n{ex.Message}\n\n" +
                    $"Программа продолжит работу в текущей версии.");
                window.SetProgress(0);

                await Task.Delay(5000);
                window.Hide();
            }
            return false;
        }
    }

    private static async Task DownloadWithProgressAsync(
        string url, string destPath, UpdateWindow window)
    {
        using var response = await _client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? -1;
        var downloadedBytes = 0L;
        var lastPercent = -1;

        await using var contentStream = await response.Content.ReadAsStreamAsync();
        await using var fileStream = new FileStream(
            destPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

        var buffer = new byte[8192];
        int bytesRead;

        while ((bytesRead = await contentStream.ReadAsync(buffer)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
            downloadedBytes += bytesRead;

            if (totalBytes > 0)
            {
                var percent = (int)(downloadedBytes * 100 / totalBytes);
                if (percent != lastPercent)
                {
                    lastPercent = percent;
                    window.SetProgress(percent);
                }
            }
        }
    }

    private static void ApplyUpdate(string tempExe, string currentExe)
    {
        var dir = Path.GetDirectoryName(currentExe)!;
        var batPath = Path.Combine(dir, "update.bat");

        var processName = Path.GetFileNameWithoutExtension(currentExe);
        var bat = "@echo off\r\n" +
                  "timeout /t 2 /nobreak >nul\r\n" +
                  $"taskkill /f /im \"{processName}.exe\" >nul 2>&1\r\n" +
                  "timeout /t 1 /nobreak >nul\r\n" +
                  $"move /y \"{tempExe}\" \"{currentExe}\" >nul\r\n" +
                  $"start \"\" \"{currentExe}\"\r\n" +
                  "del \"%~f0\"\r\n";

        File.WriteAllText(batPath, bat);

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
