using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Anemoi.Centralize.Infrastructure.Services;

public static class SshKeyGenerator
{
    public static void EnsureKeysExist(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var privateKeyPath = Path.Combine(directoryPath, "id_rsa");
        var publicKeyPath = Path.Combine(directoryPath, "id_rsa.pub");

        if (File.Exists(privateKeyPath) && File.Exists(publicKeyPath))
        {
            return; // Keys already exist
        }

        using var rsa = RSA.Create(2048);
        
        // Export Private Key in PEM format
        var privateKeyPem = rsa.ExportPkcs8PrivateKeyPem();
        File.WriteAllText(privateKeyPath, privateKeyPem, Encoding.UTF8);
        
        // On Linux/macOS, we should set private key permissions to 600 (owner read/write only)
        try
        {
            if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                File.SetUnixFileMode(privateKeyPath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
            }
        }
        catch
        {
            // Ignore permission setting errors on systems/filesystems that don't support it
        }

        // Export Public Key in OpenSSH format
        var rsaParams = rsa.ExportParameters(false);
        var sshPublicKey = ExportToSshPublicKey(rsaParams);
        File.WriteAllText(publicKeyPath, sshPublicKey, Encoding.UTF8);
    }

    private static string ExportToSshPublicKey(RSAParameters keyParams)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        
        // Write "ssh-rsa" type
        WriteLengthAndBytes(writer, Encoding.ASCII.GetBytes("ssh-rsa"));
        
        // Write exponent
        WriteLengthAndBytes(writer, keyParams.Exponent);
        
        // Write modulus (ensuring it's treated as a signed positive integer in big-endian)
        WriteLengthAndBytes(writer, keyParams.Modulus, forcePositiveLead: true);
        
        var base64Key = Convert.ToBase64String(ms.ToArray());
        return $"ssh-rsa {base64Key} dev@anemoi";
    }

    private static void WriteLengthAndBytes(BinaryWriter writer, byte[] bytes, bool forcePositiveLead = false)
    {
        var list = new List<byte>(bytes);
        
        // If the MSB of the modulus is set (i.e. >= 0x80), SSH protocol expects a leading zero byte
        // to keep the value positive (since SSH integers are signed two's complement).
        if (forcePositiveLead && (bytes[0] & 0x80) != 0)
        {
            list.Insert(0, 0);
        }

        var finalBytes = list.ToArray();
        var length = finalBytes.Length;
        
        byte[] lenBytes = BitConverter.GetBytes(length);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(lenBytes);
        }
        
        writer.Write(lenBytes);
        writer.Write(finalBytes);
    }
}
