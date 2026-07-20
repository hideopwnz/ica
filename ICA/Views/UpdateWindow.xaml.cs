using System.Windows;

namespace ICA.Views;

public partial class UpdateWindow : Window
{
    public UpdateWindow()
    {
        InitializeComponent();
        PositionBottomRight();
    }

    private void PositionBottomRight()
    {
        var workArea = SystemParameters.WorkArea;
        Left = workArea.Right - Width - 16;
        Top = workArea.Bottom - Height - 16;
    }

    public void SetStatus(string text)
    {
        StatusText.Text = text;
    }

    public void SetProgress(double percent)
    {
        Progress.Value = percent;
        PercentText.Text = percent >= 100 ? "100%" : $"{percent:F0}%";
    }
}
