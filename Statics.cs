namespace Eclipse;

public static class Statics
{
    public static bool textchanged { get; set; } = false;

    public static bool ipv6 = false;

    public static bool ToReceive = true;

    public static bool IsNull = true;

    private static IPEndPoint _ip = new(IPAddress.Any, 8085);

    public static IPEndPoint ip
    {
        get => _ip;

        set => _ip = value;
    }

#nullable disable

    public static UdpClient client4;

    public static UdpClient client6;

#nullable restore

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

    public const string MMPFile = "MlineMesProto_File";
}

public class MMPBuilder
{
    public MMPBuilder(MMPIdentifier identifier) => this.Append(identifier.ToString());

    private List<byte[]> data = new();

    private int count;

    public MMPBuilder Append(byte[] bytes)
    {
        data.Add(bytes);

        count += bytes.Length;

        return this;
    }

    public MMPBuilder Append(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);

        data.Add(bytes);

        count += bytes.Length;

        return this;
    }

    public int Count
    {
        get => count;
    }

    public static implicit operator byte[](MMPBuilder builder)
    {
        IEnumerable<byte> list = new byte[0];

        builder.data.ForEach(x => list = list.Concat(x));

        return list.ToArray();
    }

    public static implicit operator int(MMPBuilder builder) => builder.Count;
}

public enum MMPIdentifier
{
    MlineMesProto_Text,

    MlineMesProto_File
}