using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using ICA.Models;
using ICA.ViewModels;

namespace ICA.Views;

public partial class WidgetWindow : Window
{
    private readonly WidgetViewModel _viewModel;
    private readonly DispatcherTimer _autoHideTimer;

    public WidgetWindow(WidgetViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        PositionBottomRight();

        _autoHideTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(3)
        };
        _autoHideTimer.Tick += (s, e) =>
        {
            _autoHideTimer.Stop();
            FadeOutAndHide();
        };

        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName != nameof(WidgetViewModel.Status)) return;

            if (_viewModel.Status == ConnectionStatus.Online)
            {
                _autoHideTimer.Stop();
                _autoHideTimer.Start();
            }
            else
            {
                _autoHideTimer.Stop();
            }
        };

        IsVisibleChanged += (s, e) =>
        {
            if ((bool)e.NewValue) FadeIn();
        };
    }

    private void PositionBottomRight()
    {
        var workArea = SystemParameters.WorkArea;
        Left = workArea.Right - Width - 16;
        Top = workArea.Bottom - Height - 16;
    }

    private void FadeIn()
    {
        Opacity = 0;
        BeginAnimation(OpacityProperty,
            new DoubleAnimation(1, TimeSpan.FromMilliseconds(200)));
    }

    private void FadeOutAndHide()
    {
        var anim = new DoubleAnimation(0, TimeSpan.FromMilliseconds(300));
        anim.Completed += (s, e) => Hide();
        BeginAnimation(OpacityProperty, anim);
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _autoHideTimer.Stop();
        Hide();
    }
}

