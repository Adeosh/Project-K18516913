using Amazon.S3;
using Amazon.S3.Model;
using Krepim.Catalog.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Krepim.Catalog.Infrastructure.Storage
{
    internal sealed class MinioFileStorageService : IFileStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly MinioOptions _options;
        private readonly ILogger<MinioFileStorageService> _logger;

        public MinioFileStorageService(IOptions<MinioOptions> options, ILogger<MinioFileStorageService> logger)
        {
            _options = options.Value;
            _logger = logger;

            var s3Config = new AmazonS3Config
            {
                ServiceURL = _options.Endpoint,
                ForcePathStyle = true // Обязательно для MinIO
            };

            _s3Client = new AmazonS3Client(_options.AccessKey, _options.SecretKey, s3Config);
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken)
        {
            try
            {
                await EnsureBucketExistsAsync(cancellationToken);

                string uniqueFileName = $"{Guid.NewGuid()}-{fileName}";
                var request = new Amazon.S3.Model.PutObjectRequest
                {
                    BucketName = _options.BucketName,
                    Key = uniqueFileName,
                    InputStream = fileStream,
                    ContentType = contentType,
                    CannedACL = S3CannedACL.PublicRead
                };

                await _s3Client.PutObjectAsync(request, cancellationToken);

                return $"{_options.Endpoint}/{_options.BucketName}/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file {FileName} to MinIO.", fileName);
                throw;
            }
        }

        private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
        {
            var bucketExists = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, _options.BucketName);
            if (!bucketExists)
            {
                await _s3Client.PutBucketAsync(new PutBucketRequest { BucketName = _options.BucketName }, cancellationToken);

                var policy = $$"""
                {
                    "Version": "2012-10-17",
                    "Statement": [
                        {
                            "Effect": "Allow",
                            "Principal": "*",
                            "Action": ["s3:GetObject"],
                            "Resource": ["arn:aws:s3:::{{_options.BucketName}}/*"]
                        }
                    ]
                }
                """;

                await _s3Client.PutBucketPolicyAsync(_options.BucketName, policy, cancellationToken);
            }
        }
    }
}
