// Examify.Infrastructure/Services/FileStorageService.cs
using Examify.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Examify.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly string _storagePath;

    public FileStorageService(IConfiguration configuration)
    {
        _storagePath = configuration["Storage:Path"] ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        // Tạo thư mục nếu chưa tồn tại
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }

    public async Task<string> UploadFileAsync(IFormFile file, string containerName)
    {
        var uploadFolder = Path.Combine(_storagePath, containerName);
        if (!Directory.Exists(uploadFolder))
        {
            Directory.CreateDirectory(uploadFolder);
        }

        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        var filePath = Path.Combine(uploadFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/{containerName}/{fileName}";
    }

    public async Task<string> UploadFileAsync(byte[] fileBytes, string fileName, string containerName)
    {
        var uploadFolder = Path.Combine(_storagePath, containerName);
        if (!Directory.Exists(uploadFolder))
        {
            Directory.CreateDirectory(uploadFolder);
        }

        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(uploadFolder, uniqueFileName);

        await File.WriteAllBytesAsync(filePath, fileBytes);

        return $"/uploads/{containerName}/{uniqueFileName}";
    }

    public Task DeleteFileAsync(string fileUrl)
    {
        var relativePath = fileUrl.TrimStart('/');
        var fullPath = Path.Combine(_storagePath, relativePath.Replace("uploads/", ""));

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public string GetFileUrl(string fileName, string containerName)
    {
        return $"/uploads/{containerName}/{fileName}";
    }
}