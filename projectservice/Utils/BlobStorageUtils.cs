using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Security.Cryptography;
using System.IO;

namespace projectservice.Utils
{
    public class BlobStorageUtils : IBlobStorageUtils
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;

        private BlobContainerClient _containerClient;

        public BlobStorageUtils(IConfiguration config, ILogger<BlobStorageUtils> logger)
        {
            _config = config;
            _logger = logger;
            string connectionString = config.GetSection("BlobStorage:ConnectionString").Value.ToString();
            string containerName = config.GetSection("BlobStorage:Container").Value.ToString();
            _containerClient = new BlobContainerClient(connectionString, containerName);
        }

        public async Task<bool> DeleteFileAsync(string fileName)
        {
            await _containerClient.GetBlobClient(fileName).DeleteIfExistsAsync();
            return true;
        }

        public async Task<Uri> UploadFileIfNotExistsAsync(byte[] fileData, string originalFileName, string projectId)
        {
            // Calculate MD5 hash of the file content
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(fileData);
            var hashString = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            var blobClient = _containerClient.GetBlobClient(hashString);

            // Check if a blob with the same hash exists
            if (!await blobClient.ExistsAsync())
            {
                // If not, upload the file
                using var stream = new MemoryStream(fileData);
                var blobUploadOptions = new BlobUploadOptions
                {
                    Metadata = new Dictionary<string, string>
                    {
                        { "OriginalFileName", originalFileName },
                        { "ProjectId",  projectId },
                        { "FileExtension", Path.GetExtension(originalFileName)}
                    }
                };
                await blobClient.UploadAsync(stream, blobUploadOptions);
            }
            return blobClient.Uri;
        }
    }
}

