using Microsoft.AspNetCore.Http;

namespace AIaaS.Application.Common.ExtensionMethods
{
    public static class IFormFileExtensions
    {
        public static async Task<string> SaveTempFile(this IFormFile file)
        {
            var filePath = Path.GetTempPath() + Guid.NewGuid().ToString();// + ".csv";
            using var fileStream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(fileStream);
            await fileStream.FlushAsync();

            return filePath;
        }
    }
}
