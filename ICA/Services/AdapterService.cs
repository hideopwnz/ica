using System.Net.NetworkInformation;

namespace ICA.Services;

public class AdapterService
{
    public bool IsAdapterUp()
    {
        try
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Any(ni =>
                    ni.OperationalStatus == OperationalStatus.Up &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel);
        }
        catch
        {
            return false;
        }
    }
}
