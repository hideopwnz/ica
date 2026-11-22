using System.IO;
using System.Text.Json;
using System.Windows;
using ICA.Services;
using ICA.ViewModels;
using ICA.Views;

namespace ICA;

public partial class App : Application
{
    private MonitorService? _monitor;
    private TrayService? _tray;
    private CancellationTokenSource? _cts;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var config = LoadConfig();

        var adapter = new AdapterService();
        var gateway = new GatewayPingService();
        var https = new HttpsCheckService(config.Sites);

        _monitor = new MonitorService(adapter, gateway, https,
            config.AdapterCheckIntervalMs,
            config.HttpsCheckIntervalMs,
            config.FailThreshold,
            config.SuccessThreshold);

        var autostart = new AutostartService();
        var viewModel = new WidgetViewModel(_monitor);
        var widget = new WidgetWindow(viewModel);

        _tray = new TrayService(widget, autostart, _monitor);
        _tray.Initialize();

        // Показать виджет при смене статуса
        _monitor.StatusChanged += _ =>
        {
            Dispatcher.Invoke(() => widget.Show());
        };

        // Проверка обновлений
        var updater = new UpdateService();
        await updater.CheckAndUpdateAsync();

        // Запуск мониторинга
        _cts = new CancellationTokenSource();
        _ = _monitor.StartAsync(_cts.Token);

        // Показываем виджет сразу при старте, чтобы отобразить начальный статус
        widget.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _cts?.Cancel();
        _tray?.Dispose();
        base.OnExit(e);
    }

    private static readonly string[] LegacySites =
    {
        "https://ya.ru",
        "https://www.google.com",
        "https://www.cloudflare.com"
    };

    private static AppConfig LoadConfig()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        var opts = new JsonSerializerOptions { WriteIndented = true };

        AppConfig cfg;

        if (File.Exists(path))
        {
            try
            {
                cfg = JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(path)) ?? AppConfig.Default;
            }
            catch
            {
                cfg = AppConfig.Default;
            }

            // Миграция: если набор сайтов — старый дефолт, меняем на новый RU-набор.
            // Пользовательский список не трогаем.
            if (cfg.Sites.Length == 0 ||
                cfg.Sites.SequenceEqual(LegacySites, StringComparer.OrdinalIgnoreCase))
            {
                cfg.Sites = AppConfig.Default.Sites;
                File.WriteAllText(path, JsonSerializer.Serialize(cfg, opts));
            }
        }
        else
        {
            cfg = AppConfig.Default;
            File.WriteAllText(path, JsonSerializer.Serialize(cfg, opts));
        }

        return cfg;
    }
}

public class AppConfig
{
    public string[] Sites { get; set; } = Array.Empty<string>();
    public int AdapterCheckIntervalMs { get; set; } = 1000;
    public int HttpsCheckIntervalMs { get; set; } = 3000;
    public int FailThreshold { get; set; } = 5;
    public int SuccessThreshold { get; set; } = 2;

    public static AppConfig Default => new()
    {
        Sites = new[]
        {
            "https://ya.ru",
            "https://yandex.ru/favicon.ico",
            "https://vk.com",
            "http://cp.cloudflare.com/generate_204"
        }
    };
}