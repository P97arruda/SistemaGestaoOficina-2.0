using Azure.Storage.Blobs;

namespace SistemaGestaoOficina.Helpers
{
    public class BlobHelper : IBlobHelper
    {
        private readonly BlobServiceClient _blobServiceClient;

        public BlobHelper(IConfiguration configuration)
        {
            string connectionString = configuration["Blob:BlobConnectionString"]!;

            _blobServiceClient = new BlobServiceClient(connectionString);
        }
        public async Task<Guid> UploadBlobAsync(IFormFile file, string containerName)
        {
            Guid imageId = Guid.NewGuid();

            var container = _blobServiceClient.GetBlobContainerClient(containerName);

            var blob = container.GetBlobClient(imageId.ToString());

            using var stream = file.OpenReadStream();

            await blob.UploadAsync(stream);

            return imageId;
        }
    }
}
