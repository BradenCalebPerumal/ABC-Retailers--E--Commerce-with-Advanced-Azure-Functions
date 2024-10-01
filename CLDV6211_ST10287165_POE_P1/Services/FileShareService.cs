using Azure.Storage.Files.Shares;
using CLDV6211_ST10287165_POE_P1.Models;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CLDV6211_ST10287165_POE_P1.Services
{
    public class FileShareService
    {
        private readonly ShareClient _shareClient;

        public FileShareService(string connectionString)
        {
            string shareName = "clientcontracts";  // Hardcoded share name

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));

            _shareClient = new ShareClient(connectionString, shareName);
            _shareClient.CreateIfNotExists();
        }

        public byte[] GenerateContractPdf(Client model)
        {
            Console.WriteLine("Starting PDF generation for client: " + model.Username);

            using (MemoryStream stream = new MemoryStream())
            {
                Document document = new Document(PageSize.A4);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                document.Open();

                try
                {
                    string imagePath = "\"~/images/logopdf.jpg\" "; // Update the path as needed, remove quotes and relative path
                    Console.WriteLine("Attempting to load image from: " + imagePath);

                    Image image = Image.GetInstance(imagePath);
                    image.ScaleToFit(140f, 120f);
                    image.Alignment = Element.ALIGN_CENTER;
                    document.Add(image);
                    Console.WriteLine("Image added successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error loading image: " + ex.Message);
                }

                // Add client details and contract text
                document.Add(new Paragraph($"Contract Agreement for {model.Username}"));
                Console.WriteLine("Added contract title.");

                document.Add(new Paragraph($"Name: {model.ClientFirstName} {model.LastName}"));
                document.Add(new Paragraph($"ID Number: {model.IdentityNum}"));
                document.Add(new Paragraph($"Cell Number: {model.CellNum}"));
                Console.WriteLine("Added client details.");

                document.Add(new Paragraph("\n\nTerms and Conditions"));
                document.Add(new Paragraph("1. The client agrees to subscribe to sell for 1 year."));
                document.Add(new Paragraph("2. [Other terms and conditions here]"));
                Console.WriteLine("Added terms and conditions.");

                document.Close();
                writer.Close();

                Console.WriteLine("PDF generation completed successfully.");
                return stream.ToArray();
            }
        }



        public async Task<bool> UploadFileAsync(string fileName, Stream fileStream)
        {
            try
            {
                // Extract directory name from the file path
                var directoryPath = Path.GetDirectoryName(fileName);
                if (string.IsNullOrEmpty(directoryPath))
                {
                    directoryPath = "root";  // Use a default or root directory if none is specified
                }

                var directoryClient = _shareClient.GetDirectoryClient(directoryPath);
                if (!await directoryClient.ExistsAsync())
                {
                    await directoryClient.CreateAsync();  // Create the directory if it does not exist
                }

                var fileClient = directoryClient.GetFileClient(Path.GetFileName(fileName));
                await fileClient.CreateAsync(fileStream.Length);
                await fileClient.UploadAsync(fileStream);
                return true;  // Indicate success
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading file: {ex.Message}");
                return false;  // Indicate failure
            }
        }

        public async Task<bool> UploadFileToFunction(string fileName, Stream fileStream)
        {
            using (var client = new HttpClient())
            {
                using (var content = new MultipartFormDataContent())
                {
                    content.Add(new StreamContent(fileStream), "file", Path.GetFileName(fileName));
                    content.Add(new StringContent(Path.GetDirectoryName(fileName) ?? "root"), "filePath");
                    content.Add(new StringContent(fileName), "fileName");

                    // Ensure the URL is correctly configured
                    var functionUrl = "https://st10287165bcpfucntion.azurewebsites.net/api/FileHandler";
                    var response = await client.PostAsync(functionUrl, content);

                    if (!response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"File upload failed with status code: {response.StatusCode}");
                        Console.WriteLine($"Response from server: {responseContent}");
                        return false;
                    }
                    return true;
                }
            }
        }


        public async Task<Stream> DownloadFileAsync(string fileName)
        {
            var directoryClient = _shareClient.GetRootDirectoryClient();
            var fileClient = directoryClient.GetFileClient(fileName);
            var download = await fileClient.DownloadAsync();
            return download.Value.Content;
        }

        public async Task DeleteFileAsync(string fileName)
        {
            var directoryClient = _shareClient.GetRootDirectoryClient();
            var fileClient = directoryClient.GetFileClient(fileName);
            await fileClient.DeleteIfExistsAsync();
        }
    }

}
