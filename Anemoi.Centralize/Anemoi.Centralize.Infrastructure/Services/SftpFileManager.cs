using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Anemoi.Centralize.Application.Abstractions;
using Anemoi.Centralize.Application.Configurations;

namespace Anemoi.Centralize.Infrastructure.Services;

public sealed class SftpFileManager : ISftpFileManager
{
    private readonly string _basePath;

    public SftpFileManager(DevEnvironmentsSetting settings)
    {
        // Try container path first, fallback to configured local environment directory
        _basePath = "/app/sftp_data";
        if (!Directory.Exists(_basePath))
        {
            _basePath = Path.Combine(Directory.GetCurrentDirectory(), settings.LocalEnvDir, "sftp_data");
        }

        _basePath = Path.GetFullPath(_basePath);
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    private string GetFullPath(string relativePath)
    {
        var cleaned = (relativePath ?? "").Replace('\\', '/').Trim('/');
        var combined = Path.Combine(_basePath, cleaned);
        var fullPath = Path.GetFullPath(combined);

        if (!IsWithinBasePath(fullPath))
        {
            throw new UnauthorizedAccessException("Path traversal detected.");
        }

        return fullPath;
    }

    public List<SftpFileDto> ListFiles(string relativePath)
    {
        var fullPath = GetFullPath(relativePath);
        if (!Directory.Exists(fullPath))
        {
            return new List<SftpFileDto>();
        }

        var dirInfo = new DirectoryInfo(fullPath);
        var items = dirInfo.GetFileSystemInfos();

        return items
            .Select(f =>
            {
                var relPath = Path.GetRelativePath(_basePath, f.FullName).Replace('\\', '/');
                var isDir = f is DirectoryInfo;
                var size = isDir ? 0 : ((FileInfo)f).Length;
                return new SftpFileDto(f.Name, relPath, size, isDir, f.LastWriteTimeUtc);
            })
            .ToList();
    }

    public async Task UploadFileAsync(string relativePath, string fileName, Stream fileStream)
    {
        var dirPath = GetFullPath(relativePath);
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }

        var fullFilePath = Path.Combine(dirPath, fileName);
        if (!IsWithinBasePath(Path.GetFullPath(fullFilePath)))
        {
            throw new UnauthorizedAccessException("Path traversal detected.");
        }

        using var fs = new FileStream(fullFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await fileStream.CopyToAsync(fs);
    }

    public Stream DownloadFile(string relativePath)
    {
        var fullPath = GetFullPath(relativePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("File not found.", relativePath);
        }

        return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    public bool DeleteFile(string relativePath)
    {
        var fullPath = GetFullPath(relativePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            return true;
        }
        if (Directory.Exists(fullPath))
        {
            Directory.Delete(fullPath, recursive: true);
            return true;
        }
        return false;
    }

    public bool CreateDirectory(string relativePath)
    {
        var fullPath = GetFullPath(relativePath);
        if (Directory.Exists(fullPath))
        {
            return false;
        }
        Directory.CreateDirectory(fullPath);
        return true;
    }

    private bool IsWithinBasePath(string path)
    {
        var relativePath = Path.GetRelativePath(_basePath, path);
        return !Path.IsPathRooted(relativePath) &&
               relativePath != ".." &&
               !relativePath.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal) &&
               !relativePath.StartsWith($"..{Path.AltDirectorySeparatorChar}", StringComparison.Ordinal);
    }
}
