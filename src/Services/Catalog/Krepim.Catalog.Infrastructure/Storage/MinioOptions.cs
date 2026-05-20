namespace Krepim.Catalog.Infrastructure.Storage
{
    public sealed class MinioOptions
    {
        public string Endpoint { get; init; } = string.Empty;
        public string AccessKey { get; init; } = string.Empty;
        public string SecretKey { get; init; } = string.Empty;
        public string BucketName { get; init; } = "catalog-images";
    }
}
