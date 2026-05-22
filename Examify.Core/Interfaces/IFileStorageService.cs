// Examify.Core/Interfaces/IFileStorageService.cs
using Microsoft.AspNetCore.Http;

namespace Examify.Core.Interfaces;

public interface IFileStorageService
{
    /// <summary>
    /// Upload file lên storage
    /// </summary>
    Task<string> UploadFileAsync(IFormFile file, string containerName);

    /// <summary>
    /// Upload file từ byte array
    /// </summary>
    Task<string> UploadFileAsync(byte[] fileBytes, string fileName, string containerName);

    /// <summary>
    /// Xóa file khỏi storage
    /// </summary>
    Task DeleteFileAsync(string fileUrl);

    /// <summary>
    /// Lấy URL của file
    /// </summary>
    string GetFileUrl(string fileName, string containerName);
}