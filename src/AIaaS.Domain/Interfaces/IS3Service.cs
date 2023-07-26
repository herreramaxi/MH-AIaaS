namespace AIaaS.Domain.Interfaces
{
    public interface IS3Service
    {
        Task<bool> UploadFileAsync(Stream fileStream, string key);
        Task<Stream?> DownloadFileAsync(string key);
        Task<bool> DeleteFileAsync(string key);
    }
}
