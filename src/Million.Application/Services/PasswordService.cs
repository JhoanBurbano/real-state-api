using System.Text;

namespace Million.Application.Services;

public interface IPasswordService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

public class PasswordService : IPasswordService
{
    public string HashPassword(string password)
    {
        var argon2 = new Konscious.Security.Cryptography.Argon2id(Encoding.UTF8.GetBytes(password))
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

    public bool VerifyPassword(string password, string hash)
    {
        try
        {
            var combined = Convert.FromBase64String(hash);
            var salt = new byte[16];
            var storedHash = new byte[32];

            Array.Copy(combined, 0, salt, 0, salt.Length);
            Array.Copy(combined, salt.Length, storedHash, 0, storedHash.Length);

            var argon2 = new Konscious.Security.Cryptography.Argon2id(Encoding.UTF8.GetBytes(password))
            {
                DegreeOfParallelism = 8,
                MemorySize = 65536,
                Iterations = 3,
                Salt = salt
            };

            var computedHash = argon2.GetBytes(32);
            return storedHash.SequenceEqual(computedHash);
        }
        catch
        {
            return false;
        }
    }
}
