using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace FunctionApp1
{
    public class UploadBlobFromUrlFunction
    {
        private readonly ILogger<UploadBlobFromUrlFunction> _logger;

        public UploadBlobFromUrlFunction(ILogger<UploadBlobFromUrlFunction> logger)
        {
            _logger = logger;
        }

        [Function("UploadBlobFromUrl")]
        public static async Task<IActionResult> Run(
          [Microsoft.Azure.Functions.Worker.HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
          ILogger log)
        {
            string imageUrl = req.Query["imageUrl"];
            string containerName = req.Query["containerName"];
            string blobName = req.Query["blobName"];

            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(imageUrl);
            if (!response.IsSuccessStatusCode)
            {
                return new BadRequestObjectResult("Failed to download image from URL.");
            }

            var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(blobName);
            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                await blobClient.UploadAsync(stream, new Azure.Storage.Blobs.Models.BlobHttpHeaders { ContentType = response.Content.Headers.ContentType.ToString() });
            }

            string uploadedUri = blobClient.Uri.ToString();
            return new OkObjectResult(uploadedUri);
        }
    }
}
