using System.Text;
using Konscious.Security.Cryptography;

class Program
{
    static void Main()
    {
        var password = "12345678";
        var hash = HashPassword(password);
        Console.WriteLine($"Password: {password}");
        Console.WriteLine($"Hash: {hash}");
    }

    static string HashPassword(string password)
    {
        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            DegreeOfParallelism = 8,
            MemorySize = 65536, // 64MB
            Iterations = 3
        };

        var salt = new byte[16];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(salt);

        argon2.Salt = salt;

        var hash = argon2.GetBytes(32);
        var combined = new byte[salt.Length + hash.Length];
        Array.Copy(salt, 0, combined, 0, salt.Length);
        Array.Copy(hash, 0, combined, salt.Length, hash.Length);

        return Convert.ToBase64String(combined);
    }
}
