using System.Security.Cryptography;

namespace Elwise.Api.Security;

/// <summary>
/// Hashes and checks the admin password with PBKDF2.
/// The server only saves the hash. The password itself is never saved.
/// </summary>
public static class AdminPassword
{
    private const int SaltSize = 16;        // 128 bits
    private const int HashSize = 32;        // 256 bits
    private const int Iterations = 600_000; // OWASP's advice for PBKDF2 with SHA-256

    /// <summary>
    /// Makes a hash in the form "iterations.salt.hash", with salt and hash in Base64.
    /// </summary>
    public static string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);

        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// Returns true if the password gives the same hash as the saved one.
    /// </summary>
    public static bool Verify(string password, string savedHash)
    {
        string[] parts = savedHash.Split('.');

        if (parts.Length != 3 || !int.TryParse(parts[0], out int iterations) || iterations <= 0)
        {
            return false;
        }

        try
        {
            byte[] salt = Convert.FromBase64String(parts[1]);
            byte[] expected = Convert.FromBase64String(parts[2]);

            if (expected.Length == 0)
            {
                return false;
            }

            byte[] actual = Rfc2898DeriveBytes.Pbkdf2(
                password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);

            // Takes the same time whether the first byte or the last byte is wrong.
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException)
        {
            // The saved hash is not valid Base64.
            return false;
        }
    }
}