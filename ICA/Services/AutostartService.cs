using Microsoft.Win32;

namespace ICA.Services;

public class AutostartService
{
    private const string KeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "ICA";

    public bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(KeyPath);
        return key?.GetValue(AppName) != null;
    }

    public void Set(bool enable)
    {
        using var key = Registry.CurrentUser.OpenSubKey(KeyPath, writable: true);
        if (key == null) return;

        if (enable)
            key.SetValue(AppName, Environment.ProcessPath!);
        else
            key.DeleteValue(AppName, throwOnMissingValue: false);
    }
}
