using AIaaS.Domain.Interfaces;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Ardalis.Result;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;

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

        public string? GetS3ResourceUrl(string? key)
        {
            return !string.IsNullOrEmpty(key) ? $"https://{_bucketName}.s3.eu-west-1.amazonaws.com/{key}" : key;
        }

        public async Task<Result> UploadFileAsync(Stream fileStream, string key, bool makePublic = false)
        {
            try
            {
                using var transferUtility = new TransferUtility(_s3Client);
                await transferUtility.UploadAsync(fileStream, _bucketName, key);

                if (makePublic)
                {
                    await _s3Client.MakeObjectPublicAsync(_bucketName, key, true);
                }

                return Result.Success();
            }
            catch (Exception exception)
            {
                var errorMessage = $"Error when trying to upload file '{key}' to S3";
                _logger.LogError(exception, errorMessage);
                return Result.Error(errorMessage);
            }
        }

        public async Task<Result<Stream>> DownloadFileAsync(string key)
        {
            if (string.IsNullOrEmpty(key)) return Result.Error("S3 key is required");

            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                var response = await _s3Client.GetObjectAsync(request);

                return response.HttpStatusCode == HttpStatusCode.OK ?
                    Result.Success(response.ResponseStream) :
                    Result.Error($"Not able to download file from S3, Status Code: {response.HttpStatusCode}");
            }
            catch (Exception exception)
            {
                var errorMessage = $"Error when trying to download file '{key}' from S3.";
                _logger.LogError(exception, errorMessage);
                return Result.Error(errorMessage);
            }
        }

        public async Task<Result> DeleteFileAsync(string key)
        {
            if (string.IsNullOrEmpty(key)) return Result.Error("S3 key is required");

            try
            {
                var request = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                var response = await _s3Client.DeleteObjectAsync(request);
                return response.HttpStatusCode == HttpStatusCode.NoContent ?
                       Result.Success() :
                        Result.Error($"Error when trying to delete file '{key}' from S3, Status Code: {response.HttpStatusCode}");
            }
            catch (Exception exception)
            {
                var errorMessage = $"Error when trying to delete file '{key}' from S3.";
                _logger.LogError(exception, errorMessage);
                return Result.Error(errorMessage);
            }
        }
    }
}