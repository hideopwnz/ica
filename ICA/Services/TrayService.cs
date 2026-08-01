using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using Hardcodet.Wpf.TaskbarNotification;
using ICA.Models;
using ICA.Views;

namespace ICA.Services;

public class TrayService : IDisposable
{
    private TaskbarIcon? _trayIcon;
    private readonly WidgetWindow _widget;
    private readonly AutostartService _autostart;
    private readonly MonitorService _monitor;
    private Icon? _greenIcon;
    private Icon? _redIcon;

    public TrayService(WidgetWindow widget, AutostartService autostart, MonitorService monitor)
    {
        _widget = widget;
        _autostart = autostart;
        _monitor = monitor;
    }

    public void Initialize()
    {
        _greenIcon = LoadIcon("green.ico");
        _redIcon = LoadIcon("red.ico");

        _trayIcon = new TaskbarIcon
        {
            Icon = _redIcon,
            ToolTipText = "ICA — Internet Connection Available"
        };
        _trayIcon.TrayLeftMouseUp += (s, e) => ToggleWidget();
        _trayIcon.ContextMenu = BuildContextMenu();

        _monitor.StatusChanged += OnStatusChanged;
    }

    private void OnStatusChanged(ConnectionStatus status)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (_trayIcon != null)
                _trayIcon.Icon = status == ConnectionStatus.Online ? _greenIcon : _redIcon;
        });
    }

    private void ToggleWidget()
    {
        if (_widget.IsVisible) _widget.Hide();
        else _widget.Show();
    }

    private ContextMenu BuildContextMenu()
    {
        var menu = new ContextMenu();

        var checkBox = new CheckBox
        {
            IsChecked = _autostart.IsEnabled(),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            IsHitTestVisible = false,
            Focusable = false
        };

        var autostartItem = new MenuItem
        {
            Header = "Автозапуск",
            Icon = checkBox
        };
        autostartItem.Click += (s, e) =>
        {
            checkBox.IsChecked = !checkBox.IsChecked;
            _autostart.Set(checkBox.IsChecked == true);
        };

        var exitItem = new MenuItem { Header = "Выход" };
        exitItem.Click += (s, e) =>
        {
            _trayIcon?.Dispose();
            Application.Current.Shutdown();
        };

        menu.Items.Add(autostartItem);
        menu.Items.Add(new Separator());
        menu.Items.Add(exitItem);

        return menu;
    }

    private static Icon LoadIcon(string name)
    {
        var uri = new Uri($"pack://application:,,,/Assets/{name}");
        var stream = Application.GetResourceStream(uri)!.Stream;
        return new Icon(stream);
    }

    public void Dispose()
    {
        _monitor.StatusChanged -= OnStatusChanged;
        _trayIcon?.Dispose();
    }
}
