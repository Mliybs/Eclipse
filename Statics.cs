namespace Eclipse;

public static class Statics
{
    public static bool textchanged { get; set; } = false;

    public static IPEndPoint ip { get; set; } = new(IPAddress.Any, 8085);

    public static UdpClient client = new(ip);

    public const string MMPText = "MlineMesProto_Text";
}