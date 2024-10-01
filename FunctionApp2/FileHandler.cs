using Azure;
using Azure.Storage.Files.Shares;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

public static class FileHandler
{
    [Function("FileHandler")]
    public static async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req,
        FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger("FileHandler");
        logger.LogInformation("Processing file upload request.");

        // Extract boundary from content type header
        var contentType = req.Headers.FirstOrDefault(h => h.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase)).Value.SingleOrDefault();
        var boundary = HeaderUtilities.RemoveQuotes(MediaTypeHeaderValue.Parse(contentType).Boundary).Value;

        var reader = new MultipartReader(boundary, req.Body);
        var section = await reader.ReadNextSectionAsync();

        // Get the connection string for Azure File Share from environment variables
        string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        ShareClient shareClient = new ShareClient(connectionString, "clientcontracts"); // Ensure file share is correct
        shareClient.CreateIfNotExists();

        while (section != null)
        {
            var contentDisposition = ContentDispositionHeaderValue.Parse(section.ContentDisposition);
            if (contentDisposition.DispositionType.Equals("form-data") && contentDisposition.FileName.HasValue)
            {
                var fileSection = section.AsFileSection();
                var fileName = contentDisposition.FileName.Value.Trim('"');

                // Create a directory if needed
                var directoryClient = shareClient.GetDirectoryClient("contracts");
                await directoryClient.CreateIfNotExistsAsync();
                var fileClient = directoryClient.GetFileClient(fileName);

                using (var fileStream = fileSection.FileStream)
                {
                    // Set the correct buffer size if needed
                    byte[] buffer = new byte[81920];  // 80 KB buffer for file chunks
                    int bytesRead;
                    long fileSize = fileStream.Length;

                    // Pre-allocate space for the file before uploading
                    await fileClient.CreateAsync(fileSize);

                    // Upload the file in chunks
                    while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        using (MemoryStream uploadStream = new MemoryStream(buffer, 0, bytesRead))
                        {
                            await fileClient.UploadRangeAsync(new HttpRange(fileStream.Position - bytesRead, bytesRead), uploadStream);
                        }
                    }

                    logger.LogInformation($"File {fileName} uploaded successfully to Azure File Share.");
                }
            }

            section = await reader.ReadNextSectionAsync();
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync("Files uploaded successfully.");
        return response;
    }
}
