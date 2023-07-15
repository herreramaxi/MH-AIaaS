using Microsoft.AspNetCore.Http;

namespace AIaaS.Application.Common.Models
{
    public class UploadFileStorageParameter
    {
        public int DatasetId { get; set; }
        public IFormFile File { get; set; }
    }
}
