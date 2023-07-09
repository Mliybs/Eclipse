namespace Eclipse;

public static class Statics
{
    public static string id { get; } = Generate();

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
    public MMPBuilder(MMPIdentifierOptions identifier)
    {
        this.Append(identifier);

        this.key = EncryptProvider.CreateAesKey();
    }
    
    public MMPBuilder(MMPIdentifierOptions identifier, AESKey key)
    {
        this.key = key;

        EncryptOn = true;

        this.Append(identifier);
    }
    
    public MMPBuilder(MMPIdentifierOptions identifier, string? key)
    {
        this.key = new()
        {
            Key = key ?? "00000000000000000000000000000000",

            IV = "0000000000000000"
        };

        EncryptOn = true;

        this.Append(identifier);
    }
    
    public MMPBuilder(MMPIdentifierOptions identifier, string key, string? iv)
    {
        this.key = new()
        {
            Key = key,

            IV = iv ?? "0000000000000000"
        };

        EncryptOn = true;

        this.Append(identifier);
    }

    private List<byte[]> data = new();

    private AESKey key;

    private int count;

    public bool EncryptOn { get; set; } = false;

    public int Count
    {
        get
        {
            if (count == 0)
            {
                var result = data.SelectMany(x => x).ToArray();

                if (EncryptOn)
                    return EncryptProvider.AESEncrypt(result, key.Key, key.IV).Length;

                else
                    return result.Length;
            }
            else
                return count;
        }
    }

    /// <summary>
    /// 不经过加密直接传入字节数组
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public MMPBuilder Append(byte[] bytes)
    {
        data.Add(bytes);

        return this;
    }

    public MMPBuilder Append(string content)
    {
        data.Add(Encoding.UTF8.GetBytes(content));

        return this;
    }

    public static implicit operator byte[](MMPBuilder builder)
    {
        var result = builder.data.SelectMany(x => x).ToArray();

        if (builder.EncryptOn)
        {
            result = EncryptProvider.AESEncrypt(result, builder.key.Key, builder.key.IV);

            builder.count = result.Length;

            return result;
        }
        
        else
        {
            builder.count = result.Length;

            return result;
        }
    }

    public static implicit operator int(MMPBuilder builder) => builder.Count;
}

public class MMPIdentifierOptions
{
    public MMPIdentifierOptions(MMPIdentifier identifier, object? obj = null)
    {
        switch (identifier)
        {
            case MMPIdentifier.MlineMesProto_Text:

                option = new JObject()
                {
                    {"Type", "Text"}
                }.ToString(Formatting.None);

                break;

            case MMPIdentifier.MlineMesProto_File:

                var para = obj as (string FileName, long FileSize)?;

                if (para is null)
                    throw new MMPIdentifierException("参数出错！");

                option = new JObject()
                {
                    {"Type", "File"},

                    {"FileName", para?.FileName},

                    {"FileSize", para?.FileSize}
                }.ToString(Formatting.None);

                break;

            default:

                throw new MMPIdentifierException("未知的标识符！");
        }
    }

    public string option { get; set; }

    public static implicit operator byte[](MMPIdentifierOptions options)
    {
        var bytes = Encoding.UTF8.GetBytes(options.option);

        return Encoding.UTF8.GetBytes($"MlineMesProto_{bytes.Length.ToString().PadLeft(3, '0')}").Concat(bytes).ToArray();
    }
}

public enum MMPIdentifier
{
    MlineMesProto_Text,

    MlineMesProto_File
}

public class MMPIdentifierException : ApplicationException
{
    public MMPIdentifierException()
    {}

    public MMPIdentifierException(string message)
    {
        this.message = message;
    }

    private string message = "出现MMP标识符错误！";

    public override string Message => message;
}