using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace ICA.Services;

public class GatewayPingService
{
    private const int TimeoutMs = 1000;

    public bool IsGatewayReachable()
    {
        try
        {
            var gateway = GetDefaultGateway();
            if (gateway == null)
                return false;

            using var ping = new Ping();
            var reply = ping.Send(gateway, TimeoutMs);
            return reply.Status == IPStatus.Success;
        }
        catch
        {
            return false;
        }
    }

    private static System.Net.IPAddress? GetDefaultGateway()
    {
        return NetworkInterface.GetAllNetworkInterfaces()
            .Where(ni =>
                ni.OperationalStatus == OperationalStatus.Up &&
                ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .SelectMany(ni => ni.GetIPProperties().GatewayAddresses)
            .Select(g => g.Address)
            .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
    }
}
