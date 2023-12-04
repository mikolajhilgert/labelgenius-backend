using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Azure.Storage;

namespace projectservice.Utils
{
    public class BlobStorageUtils : IBlobStorageUtils
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;

        BlobServiceClient _blobServiceClient;

        public BlobStorageUtils(IConfiguration config, ILogger<BlobStorageUtils> logger)
        {
            _config = config;
            _logger = logger; ;
            _blobServiceClient = new BlobServiceClient(config.GetSection("BlobStorage:ConnectionString").Value.ToString());
        }

        public async Task DeleteContainer(string containerName)
        {
            // Get a BlobContainerClient object 
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            // Check if the container exists
            bool isExist = await containerClient.ExistsAsync();

            // If the container does not exist, create it
            if (isExist)
            {
                await _blobServiceClient.DeleteBlobContainerAsync(containerName);
            }
            else
            {
                _logger.LogError($"Container with name '{containerName}' does not exist and thus could not be deleted.");
            }
        }

        public Task<string> GetContainerSASTokenAsync(string containerName)
        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            // Create a SAS token that's valid for 30 minutes
            BlobSasBuilder sasBuilder = new()
            {
                BlobContainerName = containerClient.Name,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(30)
            };

            // Specify read permissions for the SAS
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            // Use the key to get the SAS token
            StorageSharedKeyCredential storageSharedKeyCredential = new(_config.GetSection("BlobStorage:AccountName").Value.ToString(), _config.GetSection("BlobStorage:AccountKey").Value.ToString());
            return Task.FromResult(sasBuilder.ToSasQueryParameters(storageSharedKeyCredential).ToString());
        }

        public async Task<Dictionary<string, string>> UploadFilesAsync(List<(string originalFileName, byte[] content)> files, string projectId)
        {
            var containerClient = await GetOrCreateContainerAsync(projectId);

            Dictionary<string, string> imageUris = new();
            foreach ((string originalFileName, byte[] fileData) in files)
            {
                var fileId = Guid.NewGuid().ToString();
                // Set new random name to file
                var blobClient = containerClient.GetBlobClient(fileId + Path.GetExtension(originalFileName));

                // Create blob settings
                using var stream = new MemoryStream(fileData);
                var blobUploadOptions = new BlobUploadOptions
                {
                    Metadata = new Dictionary<string, string>
                    {
                        { "OriginalFileName", originalFileName },
                    }
                };

                // Upload blob and save name and uri
                await blobClient.UploadAsync(stream, blobUploadOptions);
                imageUris.Add(fileId, blobClient.Uri.ToString());
            }
            return imageUris;
        }

        private async Task<BlobContainerClient> GetOrCreateContainerAsync(string containerName)
        {
            // Get a BlobContainerClient object
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            // Check if the container exists
            bool isExist = await containerClient.ExistsAsync();

            // If the container does not exist, create it
            if (!isExist)
            {
                await _blobServiceClient.CreateBlobContainerAsync(containerName);
            }

            return containerClient;
        }
    }
}

