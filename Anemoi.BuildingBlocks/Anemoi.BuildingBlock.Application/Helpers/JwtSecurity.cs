using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using OneOf;

namespace Anemoi.BuildingBlock.Application.Helpers;

public static class JwtSecurity
{
    public const string DevelopmentPrivateKeyRelativePath = "Anemoi_Open/Jwt/development-private.key";
    public const string DevelopmentPublicKeyRelativePath = "Anemoi_Open/Jwt/development-public.crt";

    private const string BeginPrivateKey = "-----BEGIN RSA PRIVATE KEY-----";
    private const string EndPrivateKey = "-----END RSA PRIVATE KEY-----";
    private const string BeginPublicKey = "-----BEGIN RSA PUBLIC KEY-----";
    private const string EndPublicKey = "-----END RSA PUBLIC KEY-----";
    private const string DevelopmentEnvironmentName = "Development";
    private const string DevelopmentLockFileName = ".jwt-development.lock";

    public static string ResolveKeyPath(string configuredPath, string developmentFallbackRelativePath)
    {
        var path = string.IsNullOrWhiteSpace(configuredPath)
            ? developmentFallbackRelativePath
            : Environment.ExpandEnvironmentVariables(configuredPath);

        if (Path.IsPathRooted(path))
            return Path.GetFullPath(path);

        var rootPath = IsDevelopmentEnvironment()
            ? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            : AppContext.BaseDirectory;

        return Path.GetFullPath(Path.Combine(rootPath, path));
    }

    public static void EnsureDevelopmentKeyPair(string privateKeyPath, string publicKeyPath)
    {
        if (File.Exists(privateKeyPath) && File.Exists(publicKeyPath))
            return;

        if (!IsDevelopmentEnvironment())
        {
            throw new FileNotFoundException(
                $"JWT key pair is missing. Expected development files at '{privateKeyPath}' and '{publicKeyPath}'.");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(privateKeyPath) ?? AppContext.BaseDirectory);
        Directory.CreateDirectory(Path.GetDirectoryName(publicKeyPath) ?? AppContext.BaseDirectory);

        var lockDirectory = Path.GetDirectoryName(privateKeyPath)
            ?? Path.GetDirectoryName(publicKeyPath)
            ?? AppContext.BaseDirectory;
        var lockPath = Path.Combine(lockDirectory, DevelopmentLockFileName);

        using var lockStream = new FileStream(lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        if (File.Exists(privateKeyPath) && File.Exists(publicKeyPath))
            return;

        CreateKeyPair(privateKeyPath, publicKeyPath);
    }

    public static void EnsureDevelopmentPublicKeyExists(string publicKeyPath)
    {
        if (File.Exists(publicKeyPath))
            return;

        if (IsDevelopmentEnvironment())
        {
            throw new FileNotFoundException(
                $"JWT public key was not found at '{publicKeyPath}'. Start Identity first or ensure the shared jwt_keys volume already contains the development key pair.");
        }

        throw new FileNotFoundException(
            $"JWT public key was not found at '{publicKeyPath}'.");
    }

    public static void CreateKeyPair(string privateKeyPath, string publicKeyPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(privateKeyPath) ?? AppContext.BaseDirectory);
        Directory.CreateDirectory(Path.GetDirectoryName(publicKeyPath) ?? AppContext.BaseDirectory);

        using var rsa = RSA.Create(2048);
        File.WriteAllText(privateKeyPath, rsa.ExportRSAPrivateKeyPem(), Encoding.UTF8);
        File.WriteAllText(publicKeyPath, rsa.ExportRSAPublicKeyPem(), Encoding.UTF8);
        TrySetPrivateKeyPermissions(privateKeyPath);
    }

    public static SigningCredentials GetPrivateSigningCredential(OneOf<SecurityKey, string> secureOrPrivateKeyPath)
    {
        var rsaSecurityKey = secureOrPrivateKeyPath.Match(s => s, GetPrivateSecurityKey);
        return new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256);
    }

    public static SecurityKey GetPrivateSecurityKey(string privateKeyPath)
    {
        var privateKeyRaw = File.ReadAllText(privateKeyPath);
        var privateKey = new[] { BeginPrivateKey, EndPrivateKey }
            .Aggregate(privateKeyRaw, (acc, next) => acc.Replace(next, string.Empty));
        var privateKeyBytes = Convert.FromBase64String(privateKey);
        var rsa = RSA.Create();
        rsa.ImportRSAPrivateKey(new ReadOnlySpan<byte>(privateKeyBytes), out _);
        return new RsaSecurityKey(rsa);
    }

    public static SecurityKey GetPublicSigningCredential(string publicKeyPath)
    {
        var publicKeyRaw = File.ReadAllText(publicKeyPath);
        var publicKey = new[] { BeginPublicKey, EndPublicKey }
            .Aggregate(publicKeyRaw, (acc, next) => acc.Replace(next, string.Empty));
        var publicKeyBytes = Convert.FromBase64String(publicKey);
        var rsa = RSA.Create();
        rsa.ImportRSAPublicKey(new ReadOnlySpan<byte>(publicKeyBytes), out _);
        return new RsaSecurityKey(rsa);
    }

    private static void TrySetPrivateKeyPermissions(string privateKeyPath)
    {
        try
        {
            if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                File.SetUnixFileMode(privateKeyPath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
            }
        }
        catch
        {
            // Best effort only. Local development filesystems may not support Unix permissions.
        }
    }

    private static bool IsDevelopmentEnvironment()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        return string.Equals(environment, DevelopmentEnvironmentName, StringComparison.OrdinalIgnoreCase);
    }
}
