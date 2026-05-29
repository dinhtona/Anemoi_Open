using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Anemoi.Centralize.Application.Abstractions;

public record SftpFileDto(
    string Name,
    string Path,
    long Size,
    bool IsDirectory,
    DateTime LastModified
);

public interface ISftpFileManager
{
    List<SftpFileDto> ListFiles(string relativePath);
    Task UploadFileAsync(string relativePath, string fileName, Stream fileStream);
    Stream DownloadFile(string relativePath);
    bool DeleteFile(string relativePath);
    bool CreateDirectory(string relativePath);
}
