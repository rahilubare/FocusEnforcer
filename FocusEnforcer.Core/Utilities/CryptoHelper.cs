using System.Security.Cryptography;
using System.Text;

namespace FocusEnforcer.Core.Utilities;

public static class CryptoHelper
{
    // Use DPAPI LocalMachine so both Service (SYSTEM) and UI (User) can decrypt if they are on the same machine.
    // WARNING: Any admin on the machine can decrypt. This is a trade-off for Service/User interoperability without hardcoded keys.
    public static string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return plainText;
        byte[] data = Encoding.UTF8.GetBytes(plainText);
        byte[] encrypted = ProtectedData.Protect(data, null, DataProtectionScope.LocalMachine);
        return Convert.ToBase64String(encrypted);
    }

    public static string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return cipherText;
        byte[] data = Convert.FromBase64String(cipherText);
        byte[] decrypted = ProtectedData.Unprotect(data, null, DataProtectionScope.LocalMachine);
        return Encoding.UTF8.GetString(decrypted);
    }

    public static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
