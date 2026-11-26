using System;

public static class Base64Url
{
    public static byte[] Decode(string value)
    {
        string base64 = value
            .Replace("-", "+")
            .Replace("_", "/");

        int padding = base64.Length % 4;

        if (padding == 2)
            base64 += "==";
        else if (padding == 3)
            base64 += "=";
        else if (padding == 1)
            throw new FormatException("Invalid Base64 length.");

        return Convert.FromBase64String(base64);
    }
}