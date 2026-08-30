using FinalProject_SeventhSem.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Infrastructure.Services;

/// <summary>
/// Saves uploaded files to the local wwwroot/uploads directory.
/// Swap this implementation for a cloud provider (Azure Blob, S3) without
/// touching the Application layer.
/// </summary>
public class FileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public FileStorageService(string basePath)
        => _basePath = basePath;

    public async Task<string> SaveAsync(
        Stream fileStream, string fileName, string folder,
        CancellationToken ct = default)
    {
        var dir = Path.Combine(_basePath, folder);
        Directory.CreateDirectory(dir);

        var uniqueName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
        var fullPath = Path.Combine(dir, uniqueName);

        if (fileStream.CanSeek) fileStream.Seek(0, SeekOrigin.Begin);

        await using var output = File.Create(fullPath);
        await fileStream.CopyToAsync(output, ct);

        return $"/{folder}/{uniqueName}";
    }

    public Task DeleteAsync(string fileUrl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(fileUrl)) return Task.CompletedTask;

        var relativePath = fileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(_basePath, relativePath.Replace("uploads" + Path.DirectorySeparatorChar, ""));

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }
}
