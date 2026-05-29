// Examify.Core/Interfaces/IFileStorageService.cs
using Microsoft.AspNetCore.Http;

namespace Examify.Core.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(IFormFile file, string containerName);
    Task<string> UploadFileAsync(byte[] fileBytes, string fileName, string containerName);
    Task DeleteFileAsync(string fileUrl);
    string GetFileUrl(string fileName, string containerName);
}