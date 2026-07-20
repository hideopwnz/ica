using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using ICA.Models;
using ICA.Services;

namespace ICA.ViewModels;

public partial class WidgetViewModel : ObservableObject
{
    private readonly MonitorService _monitor;
    private bool _isFirstStatus = true;

    [ObservableProperty]
    private ConnectionStatus _status = ConnectionStatus.Offline;

    [ObservableProperty]
    private string _statusText = "Проверка...";

    [ObservableProperty]
    private SolidColorBrush _statusBrush = new((Color)ColorConverter.ConvertFromString("#F44336")) { Opacity = 0.95 };

    public WidgetViewModel(MonitorService monitor)
    {
        _monitor = monitor;
        _monitor.StatusChanged += HandleStatusChanged;
    }

    private void HandleStatusChanged(ConnectionStatus newStatus)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            Status = newStatus;

            if (newStatus == ConnectionStatus.Online)
            {
                StatusText = _isFirstStatus
                    ? "Интернет доступен"
                    : "Интернет восстановлен";
                StatusBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50")) { Opacity = 0.95 };
            }
            else
            {
                StatusText = "Интернет отсутствует";
                StatusBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F44336")) { Opacity = 0.95 };
            }

            _isFirstStatus = false;
        });
    }
}
