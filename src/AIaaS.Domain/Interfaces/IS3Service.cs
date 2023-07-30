using Ardalis.Result;

namespace AIaaS.Domain.Interfaces
{
    public interface IS3Service
    {
        string? GetS3ResourceUrl(string? key);
        Task<Result> UploadFileAsync(Stream fileStream, string key, bool makePublic = false);
        Task<Result<Stream>> DownloadFileAsync(string key);
        Task<Result> DeleteFileAsync(string key);
    }
}
