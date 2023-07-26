namespace AIaaS.Domain.Interfaces
{
    public interface IS3Service
    {
        string GetS3ResourceUrl(string? key);
        Task<bool> UploadFileAsync(Stream fileStream, string key, bool makePublic = false);
        Task<Stream?> DownloadFileAsync(string key);
        Task<bool> DeleteFileAsync(string key);
    }
}
