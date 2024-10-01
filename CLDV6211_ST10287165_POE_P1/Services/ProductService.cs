using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using CLDV6211_ST10287165_POE_P1.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CLDV6211_ST10287165_POE_P1.Services
{
    public class ProductService
    {
        private readonly TableClient _tableClient;
        private readonly IBlobStorageService _blobStorageService;
        private readonly BlobServiceClient _blobServiceClient;

        public ProductService(string tableStorageConnectionString, string blobStorageConnectionString, BlobServiceClient blobServiceClient)
        {
            // Initialize the TableClient for Product table
            _tableClient = new TableClient(tableStorageConnectionString, "Products");
            _blobServiceClient = blobServiceClient;
            _tableClient.CreateIfNotExists();


            // Initialize the BlobStorageService (you'll need to implement this service)
            _blobStorageService = new BlobStorageService(blobStorageConnectionString);
        }
        public async Task<Product> GetProductAsync(string partitionKey, string rowKey)
        {
            try
            {
                var response = await _tableClient.GetEntityAsync<Product>(partitionKey, rowKey);
                return response.Value;  // This ensures that the product is returned if found
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                Console.WriteLine($"No product found with PartitionKey: {partitionKey} and RowKey: {rowKey}");
                return null;  // Return null if the product does not exist
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving the product: {ex.Message}");
                throw;  // Re-throw the exception to handle it further up the call stack
            }
        }
        public async Task<Product> AddProductAsync(Product product)
        {
            // Generate the RowKey first
            product.PartitionKey = "Product";
            product.RowKey = Guid.NewGuid().ToString();

            if (product.ImageType == "Upload" && product.ImageUpload != null)
            {
                // Use the RowKey as the blob name when uploading the image
                var imageUrl = await _blobStorageService.UploadBlobAsync("product-images", product.ImageUpload.OpenReadStream(), product.ImageUpload.ContentType, product.RowKey);
                product.ImageUrl = imageUrl;
                product.ImageUpload = null; // Reset to avoid sending large data to Table Storage
            }
            else if (product.ImageType == "Url" && !string.IsNullOrEmpty(product.ImageUrl))
            {
                // Pass the RowKey as the blob name when saving the image from the URL
                product.ImageUrl = await SaveImageFromUrlAsync(product.ImageUrl, product.RowKey);
            }

            await _tableClient.AddEntityAsync(product);

            return product;
        }

        public async Task<string> SaveImageFromUrlAsync(string imageUrl, string blobName)
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(imageUrl);
                if (response.IsSuccessStatusCode)
                {
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        var contentType = response.Content.Headers.ContentType.ToString();
                        return await _blobStorageService.UploadBlobAsync("product-images", stream, contentType, blobName);
                    }
                }
                else
                {
                    throw new Exception("Failed to download image from URL.");
                }
            }
        }




        // Retrieve a product by its RowKey
        public async Task<Product> GetProductByIdAsync(string rowKey)
        {
            try
            {
                var product = await _tableClient.GetEntityAsync<Product>("Product", rowKey);
                return product;
            }
            catch (RequestFailedException)
            {
                return null; // Return null if the product is not found
            }
        }

        // Retrieve all products (or by a specific PartitionKey if needed)
        public async Task<List<Product>> GetAllProductsAsync()
        {
            var products = new List<Product>();
            await foreach (var product in _tableClient.QueryAsync<Product>())
            {
                // Enhanced logging
                Console.WriteLine($"Product Retrieved: {product.Name}, Price: {product.Price}");

                // Check if the Price is correctly retrieved
                if (product.Price == 0)
                {
                    Console.WriteLine($"Warning: Product '{product.Name}' retrieved with Price = 0.");
                }

                products.Add(product);
            }
            return products;
        }

        public async Task UpdateProductAsync(Product product)
        {
            try
            {
                Console.WriteLine($"Attempting to update product with RowKey: {product.RowKey}");
                Console.WriteLine($"Product Details: Name={product.Name}, Description={product.Description}, Price={product.Price}, Category={product.Category}, InStock={product.InStock}");

                // Update the existing product in Table Storage
                await _tableClient.UpdateEntityAsync(product, ETag.All, TableUpdateMode.Replace);

                Console.WriteLine("Product update successful.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating product with RowKey {product.RowKey}: {ex.Message}");
                throw;
            }
        }
        public async Task DeleteProductAsync(string partitionKey, string rowKey)
        {
            try
            {
                await _tableClient.DeleteEntityAsync(partitionKey, rowKey);
            }
            catch (RequestFailedException ex)
            {
                // Log error or handle exception
                throw;
            }
        }
        public async Task<bool> UpdateItemQuantityAsync(string userId, string rowKey, int quantity)
        {
            var existingItem = await _tableClient.GetEntityAsync<CartItem>(userId, rowKey);

            if (existingItem != null)
            {
                existingItem.Value.Quantity = quantity;
                await _tableClient.UpdateEntityAsync(existingItem.Value, existingItem.Value.ETag, TableUpdateMode.Replace);
                return true;
            }

            return false;
        }


        public async Task<Product> GetProductByKeysAsync(string partitionKey, string rowKey)
        {
            try
            {
                var product = await _tableClient.GetEntityAsync<Product>(partitionKey, rowKey);
                return product.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                // Handle not found exception
                return null;
            }
        }


        public async Task DeleteBlobAsync(string containerName, string blobName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            await blobClient.DeleteIfExistsAsync();
            Console.WriteLine("Blob " + blobName + " deleted from container " + containerName);
        }

        // Delete a product by its RowKey
        public async Task DeleteProductAsync(string rowKey)
        {
            await _tableClient.DeleteEntityAsync("Product", rowKey);
        }

        public async Task<List<Product>> GetProductsByClientIdAsync(string clientId)
        {
            var products = new List<Product>();

            // Query the products by the client ID (assuming it's stored in the PartitionKey or another property)
            await foreach (var product in _tableClient.QueryAsync<Product>(filter: $"ClientId eq '{clientId}'"))
            {
                products.Add(product);
            }

            return products;
        }

        public async Task<string> UploadProductImageAsync(IFormFile imageFile, string blobName)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                throw new ArgumentException("No image file provided", nameof(imageFile));
            }

            using (var stream = imageFile.OpenReadStream())
            {
                var contentType = imageFile.ContentType;
                // Use the blobName (RowKey) as the name of the uploaded blob
                var imageUrl = await _blobStorageService.UploadBlobAsync("product-images", stream, contentType, blobName);
                return imageUrl;
            }
        }



        // Assuming ProductService and other dependencies are already injected and set up
        public async Task<Product> AdddProductAsync(Product product)
        {
            // Generate the RowKey first
            product.PartitionKey = "Product";
            product.RowKey = Guid.NewGuid().ToString();

            if (product.ImageType == "Upload" && product.ImageUpload != null)
            {
                // Use the RowKey as the blob name when uploading the image
                var imageUrl = await UploaddImageFileAsync(product.ImageUpload, product.RowKey);
                product.ImageUrl = imageUrl;
                product.ImageUpload = null; // Reset to avoid sending large data to Table Storage
            }
            else if (product.ImageType == "Url" && !string.IsNullOrEmpty(product.ImageUrl))
            {
                // Pass the RowKey as the blob name when saving the image from the URL
                var imageUrl = await UploadImageeFromUrlAsync(product.ImageUrl, product.RowKey);
                product.ImageUrl = imageUrl;
            }

            await _tableClient.AddEntityAsync(product);
            return product;
        }

        private async Task<string> UploaddImageFileAsync(IFormFile file, string blobName)
        {
            string azureFunctionUrl = "https://st10287165bcpfucntion.azurewebsites.net/api/UploadBlob";
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("containerName", "product-images");
                    client.DefaultRequestHeaders.Add("contentType", file.ContentType);
                    client.DefaultRequestHeaders.Add("blobName", blobName);

                    memoryStream.Position = 0; // Reset stream position to the beginning
                    var response = await client.PostAsync(azureFunctionUrl, new StreamContent(memoryStream));
                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsStringAsync();
                    }
                }
            }
            return null;
        }

        private async Task<string> UploadImageeFromUrlAsync(string imageUrl, string blobName)
        {
            string azureFunctionUrl = $"https://st10287165bcpfucntion.azurewebsites.net.net/api/UploadBlobFromUrlFunction?imageUrl={Uri.EscapeDataString(imageUrl)}&containerName=product-images&blobName={blobName}";
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(azureFunctionUrl, null);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }
            return null;
        }



    }
}
