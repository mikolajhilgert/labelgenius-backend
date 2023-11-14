using Azure.Storage.Blobs;
using System;
using System.IO;
using System.Threading.Tasks;

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

        public async Task<Uri> UploadFileAsync(byte[] fileData, string fileName, string projectName)
        {
            _logger.LogInformation("Uploading project images");
            var newFileName = projectName + "-" + Guid.NewGuid().ToString() + Path.GetExtension(fileName);
            BlobClient blobClient = _containerClient.GetBlobClient(newFileName);
            using var stream = new MemoryStream(fileData);
            await blobClient.UploadAsync(stream, true);
            return blobClient.Uri;
        }
    }
}

