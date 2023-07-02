namespace Eclipse;

public static class Statics
{
    public static bool textchanged { get; set; } = false;

    public static bool ipv6 = false;

    public static bool ToReceive = true;

    private static IPEndPoint _ip = new(IPAddress.Any, 8082);

    public static IPEndPoint ip
    {
        get => _ip;

        set => _ip = value;
    }

    public static UdpClient client4 = new(ip.Port, AddressFamily.InterNetwork);

    public static UdpClient client6 = new(ip.Port, AddressFamily.InterNetworkV6);

    public static UdpClient client
    {
        get => ipv6 ? client6 : client4;

        set
        {
            client4 = value;

            client6 = value;
        }
    }

    public static void ClientClose()
    {
        client4.Close();

        client6.Close();
    }

    public static void NewClient()
    {
        client4 = new(ip.Port, AddressFamily.InterNetwork);

        client6 = new(ip.Port, AddressFamily.InterNetworkV6);
    }

    public static void BeginReceive(AsyncCallback callback, IPEndPoint ip)
    {
        client4.BeginReceive(callback, (client4, ip));

        client6.BeginReceive(callback, (client6, ip));
    }

    public const string MMPText = "MlineMesProto_Text";
}