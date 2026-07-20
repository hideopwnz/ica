using ICA.Models;

namespace ICA.Services;

public class MonitorService
{
    private readonly AdapterService _adapter;
    private readonly GatewayPingService _gateway;
    private readonly HttpsCheckService _https;

    private readonly int _failThreshold;
    private readonly int _successThreshold;
    private readonly int _fastIntervalMs;
    private readonly int _mainIntervalMs;

    private bool _fastOk;
    private bool _httpsOk;
    private int _gatewayFailCount;
    private int _httpsFailCount;
    private int _httpsSuccessCount;

    private ConnectionStatus _currentStatus = ConnectionStatus.Offline;
    private readonly object _lock = new();

    public event Action<ConnectionStatus>? StatusChanged;

    public ConnectionStatus CurrentStatus
    {
        get { lock (_lock) return _currentStatus; }
    }

    public MonitorService(
        AdapterService adapter,
        GatewayPingService gateway,
        HttpsCheckService https,
        int fastIntervalMs = 1000,
        int mainIntervalMs = 3000,
        int failThreshold = 3,
        int successThreshold = 2)
    {
        _adapter = adapter;
        _gateway = gateway;
        _https = https;
        _fastIntervalMs = fastIntervalMs;
        _mainIntervalMs = mainIntervalMs;
        _failThreshold = failThreshold;
        _successThreshold = successThreshold;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        var fast = RunFastLoopAsync(ct);
        var main = RunMainLoopAsync(ct);

        try { await Task.WhenAll(fast, main); }
        catch (OperationCanceledException) { }
    }

    private async Task RunFastLoopAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(_fastIntervalMs));

        while (await timer.WaitForNextTickAsync(ct))
        {
            bool ok;

            if (!_adapter.IsAdapterUp())
            {
                ok = false;
                _gatewayFailCount = 0;
            }
            else if (!_gateway.IsGatewayReachable())
            {
                _gatewayFailCount++;
                ok = _gatewayFailCount < 2;
            }
            else
            {
                _gatewayFailCount = 0;
                ok = true;
            }

            lock (_lock)
            {
                _fastOk = ok;
                EvaluateStatus();
            }
        }
    }

    private async Task RunMainLoopAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(_mainIntervalMs));

        while (await timer.WaitForNextTickAsync(ct))
        {
            bool ok = await _https.IsInternetAvailableAsync();

            lock (_lock)
            {
                if (ok)
                {
                    _httpsSuccessCount++;
                    _httpsFailCount = 0;
                    _httpsOk = _httpsSuccessCount >= _successThreshold;
                }
                else
                {
                    _httpsFailCount++;
                    _httpsSuccessCount = 0;

                    if (_httpsFailCount >= _failThreshold)
                        _httpsOk = false;
                }

                EvaluateStatus();
            }
        }
    }

    private void EvaluateStatus()
    {
        var newStatus = (_fastOk && _httpsOk)
            ? ConnectionStatus.Online
            : ConnectionStatus.Offline;

        if (newStatus != _currentStatus)
        {
            _currentStatus = newStatus;
            StatusChanged?.Invoke(newStatus);
        }
    }
}
