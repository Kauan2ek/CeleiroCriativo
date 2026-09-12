using System.Security.Cryptography;

namespace APICeleiroCriativo.Security;

/// <summary>
/// Hash de senha via PBKDF2 (sem dependências externas). Formato armazenado:
/// "{iterações}.{salt em base64}.{hash em base64}".
/// </summary>
public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;

    public static string Hash(string senha)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(senha, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string senha, string senhaArmazenada)
    {
        var partes = senhaArmazenada.Split('.');
        if (partes.Length != 3) return false;

        var iterations = int.Parse(partes[0]);
        var salt = Convert.FromBase64String(partes[1]);
        var hashEsperado = Convert.FromBase64String(partes[2]);

        var hashCalculado = Rfc2898DeriveBytes.Pbkdf2(senha, salt, iterations, HashAlgorithmName.SHA256, hashEsperado.Length);
        return CryptographicOperations.FixedTimeEquals(hashCalculado, hashEsperado);
    }
}
