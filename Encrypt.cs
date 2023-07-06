namespace Eclipse;

public static class StaticsEncrypt
{
    public static void Deconstruct
    (
        this AESKey key,
        out string Key,
        out string IV
    )
    {
        Key = key.Key;

        IV = key.IV;
    }

    public static void Deconstruct
    (
        this RSAKey key,
        out string PublicKey,
        out string PrivateKey
    )
    {
        PublicKey = key.PublicKey;

        PrivateKey = key.PrivateKey;
    }

    public static void Deconstruct
    (
        this RSAKey key,
        out string PublicKey,
        out string PrivateKey,
        out string Exponent,
        out string Modulus
    )
    {
        PublicKey = key.PublicKey;

        PrivateKey = key.PrivateKey;

        Exponent = key.Exponent;

        Modulus = key.Modulus;
    }
}