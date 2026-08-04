using System.Security.Cryptography;
using SolarSystem.Application.Common.Interfaces;

namespace SolarSystem.Infrastructure.Services;

/// <summary>
/// PBKDF2-HMAC-SHA256 com salt por usuario, conforme OWASP Password Storage Cheat Sheet.
/// O hash carrega o algoritmo e o numero de iteracoes para que o custo possa subir no
/// futuro sem invalidar as senhas ja gravadas.
/// Formato: pbkdf2$sha256$&lt;iteracoes&gt;$&lt;salt-b64&gt;$&lt;hash-b64&gt;
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int DefaultIterations = 210_000;
    private const string Prefix = "pbkdf2";
    private const string Algorithm = "sha256";

    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Senha não pode ser vazia.", nameof(password));

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Derive(password, salt, DefaultIterations);

        return string.Join('$',
            Prefix,
            Algorithm,
            DefaultIterations.ToString(),
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordHash))
            return false;

        var parts = passwordHash.Split('$');
        if (parts.Length != 5 || parts[0] != Prefix || parts[1] != Algorithm)
            return false;

        if (!int.TryParse(parts[2], out var iterations) || iterations <= 0)
            return false;

        byte[] salt, expected;
        try
        {
            salt = Convert.FromBase64String(parts[3]);
            expected = Convert.FromBase64String(parts[4]);
        }
        catch (FormatException)
        {
            return false;
        }

        var actual = Derive(password, salt, iterations, expected.Length);

        // Comparacao em tempo constante: nao vaza quantos bytes bateram.
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    private static byte[] Derive(string password, byte[] salt, int iterations, int size = HashSize)
        => Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, size);
}
