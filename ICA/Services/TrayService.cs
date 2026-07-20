using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using Hardcodet.Wpf.TaskbarNotification;
using ICA.Views;

namespace ICA.Services;

public class TrayService : IDisposable
{
    private TaskbarIcon? _trayIcon;
    private readonly WidgetWindow _widget;
    private readonly AutostartService _autostart;

    public TrayService(WidgetWindow widget, AutostartService autostart)
    {
        _widget = widget;
        _autostart = autostart;
    }

    public void Initialize()
    {
        _trayIcon = new TaskbarIcon
        {
            Icon = LoadIcon(),
            ToolTipText = "ICA — Internet Connection Available"
        };

        _trayIcon.TrayLeftMouseUp += (s, e) => ToggleWidget();
        _trayIcon.ContextMenu = BuildContextMenu();
    }

    private void ToggleWidget()
    {
        if (_widget.IsVisible)
            _widget.Hide();
        else
            _widget.Show();
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

    private static Icon LoadIcon()
    {
        var uri = new Uri("pack://application:,,,/Assets/icon.ico");
        var stream = Application.GetResourceStream(uri)!.Stream;
        return new Icon(stream);
    }

    public void Dispose()
    {
        _trayIcon?.Dispose();
    }
}
