using AIaaS.Domain.Interfaces;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AIaaS.Infrastructure.AWS
{
    public class S3Service : IS3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly ILogger<S3Service> _logger;
        private readonly string? _bucketName;

        public S3Service(IAmazonS3 s3Client, IConfiguration configuration, ILogger<S3Service> logger)
        {
            _s3Client = s3Client;
            _logger = logger;
            _bucketName = configuration["AWS_BUCKET_NAME"];
        }

        public async Task<bool> UploadFileAsync(Stream fileStream, string key)
        {
            try
            {
                using var transferUtility = new TransferUtility(_s3Client);
                await transferUtility.UploadAsync(fileStream, _bucketName, key);
                return true;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Error when trying to upload file '{key}' to S3");
                return false;
            }
        }

        public async Task<Stream?> DownloadFileAsync(string key)
        {
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                var response = await _s3Client.GetObjectAsync(request);
                var responseStream = response.ResponseStream;
                return responseStream;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Error when trying to download file '{key}' from S3.");
                return null;
            }
        }

        public async Task<bool> DeleteFileAsync(string key)
        {
            try
            {
                var request = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            var response = await _s3Client.DeleteObjectAsync(request);
            return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Error when trying to delete file '{key}' from S3.");
                return false;
            }
        }
    }
}