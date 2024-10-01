using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CLDV6211_ST10287165_POE_P1.Models;
using CLDV6211_ST10287165_POE_P1.Services;
using Microsoft.AspNetCore.Http;
using Azure.Data.Tables;

namespace CLDV6211_ST10287165_POE_P1.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ProductService _productService;
        private readonly ILogger<ProductsController> _logger;
        private readonly IBlobStorageService _blobStorageService;

        public ProductsController(ProductService productService, ILogger<ProductsController> logger, IBlobStorageService blobStorageService)
        {
            _productService = productService;
            _logger = logger;
            _blobStorageService = blobStorageService;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllProductsAsync();
            foreach (var product in products)
            {
                Console.WriteLine($"Product: {product.Name}, Price: {product.Price}");
            }
            return View(products);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(string id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return NotFound();

            return PartialView("_PartialProductDetails", product); // Return the partial view with the product
        }

        // GET: Products/EditDisplay
        public async Task<IActionResult> EditDisplay()
        {
            var isClientLoggedIn = HttpContext.Session.GetString("IsClientLoggedIn");
            if (isClientLoggedIn != "true")
            {
                return RedirectToAction("Login", "Clients");
            }

            var clientId = HttpContext.Session.GetString("ClientId");
            if (string.IsNullOrEmpty(clientId))
            {
                return NotFound("Client not found");
            }

            var clientProducts = await _productService.GetProductsByClientIdAsync(clientId);
            if (clientProducts == null || clientProducts.Count == 0)
            {
                ViewBag.Message = "No products listed.";
            }

            return View(clientProducts);
        }

        // GET: Products/Edit/{id}
        public async Task<IActionResult> Edit(string id)
        {
            Console.WriteLine($"Edit GET called with id: {id}");

            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Ensure these fields are set
            ViewData["RowKey"] = product.RowKey;
            ViewData["ClientId"] = product.ClientId;
            ViewData["PartitionKey"] = product.PartitionKey;

            return View(product);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Product updatedProduct)
        {
            Console.WriteLine($"Edit POST called with id: {id}");

            // Retrieve the existing product based on the RowKey (id)
            var existingProduct = await _productService.GetProductByIdAsync(id);
            if (existingProduct == null)
            {
                Console.WriteLine($"No existing product found with id: {id}. Returning NotFound.");
                return NotFound();
            }

            // Remove validation for fields that aren't needed from the form
            ModelState.Remove(nameof(updatedProduct.RowKey));
            ModelState.Remove(nameof(updatedProduct.ClientId));
            ModelState.Remove(nameof(updatedProduct.PartitionKey));

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is invalid. Errors:");
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    if (state.Errors.Any())
                    {
                        Console.WriteLine($"Key: {key}");
                        foreach (var error in state.Errors)
                        {
                            Console.WriteLine($"Error: {error.ErrorMessage}");
                        }
                    }
                }
                return View(updatedProduct);
            }

            try
            {
                // Handle image replacement if necessary
                if (updatedProduct.ImageType == "Upload" && updatedProduct.ImageUpload != null)
                {
                    Console.WriteLine($"ImageType is Upload. Replacing image for product with id: {id}");

                    // Delete the old image from Blob Storage
                    await _blobStorageService.DeleteBlobAsync("product-images", id);

                    // Upload the new image using the existing RowKey (id)
                    var newImageUrl = await _blobStorageService.UploadBlobAsync("product-images", updatedProduct.ImageUpload.OpenReadStream(), updatedProduct.ImageUpload.ContentType, id);

                    // Add cache-busting query string to the new image URL
                    existingProduct.ImageUrl = $"{newImageUrl}?v={Guid.NewGuid()}";

                    Console.WriteLine("Image upload successful.");
                }
                else if (updatedProduct.ImageType == "Url" && !string.IsNullOrEmpty(updatedProduct.ImageUrl))
                {
                    Console.WriteLine($"ImageType is Url. Replacing image for product with id: {id}");

                    // Delete the old image from Blob Storage
                    await _blobStorageService.DeleteBlobAsync("product-images", id);

                    // Download the image from the URL and upload it to Blob Storage
                    var newImageUrl = await _productService.SaveImageFromUrlAsync(updatedProduct.ImageUrl, id);

                    // Add cache-busting query string to the new image URL
                    existingProduct.ImageUrl = $"{newImageUrl}?v={Guid.NewGuid()}";

                    Console.WriteLine("Image from URL saved successfully.");
                }
                else
                {
                    // No change to the image, retain the existing one with cache busting
                    existingProduct.ImageUrl = $"{existingProduct.ImageUrl}?v={Guid.NewGuid()}";
                    Console.WriteLine("No image change detected. Retaining existing image.");
                }

                // Update the other fields
                // Update the other fields
                existingProduct.Name = updatedProduct.Name;
                existingProduct.Description = updatedProduct.Description;
                existingProduct.Price = updatedProduct.Price;
                existingProduct.Category = updatedProduct.Category;
                existingProduct.Quantity = updatedProduct.Quantity; // Handle quantity update
                                                                    // existingProduct.InStock = existingProduct.Quantity > 0 ? "In Stock" : "Out of Stock"; // Update InStock based on the new quantity

                await _productService.UpdateProductAsync(existingProduct);

                Console.WriteLine("Product update successful.");
                return RedirectToAction("EditDisplay", "Products");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating product: {ex.Message}");
                _logger.LogError("Error updating product: {0}", ex.Message);
                ModelState.AddModelError("", "An error occurred while updating the product. Please try again.");
            }

            return View(updatedProduct);
        }


        // GET: Products/Create
        public IActionResult Create()
        {
            var clientId = HttpContext.Session.GetString("ClientId");
            if (string.IsNullOrEmpty(clientId))
            {
                return RedirectToAction("Login", "Clients");
            }

            ViewData["ClientId"] = clientId;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            Console.WriteLine("Create method called.");
            var clientId = HttpContext.Session.GetString("ClientId");
            Console.WriteLine($"ClientId from session: {clientId}");

            if (string.IsNullOrEmpty(clientId))
            {
                Console.WriteLine("Client is not logged in.");
                ModelState.AddModelError("", "You must be logged in to add products.");
                return RedirectToAction("Login", "Clients");
            }

            // Assign the required values before validation
            product.PartitionKey = "Product";  // Fixed partition key
            product.RowKey = Guid.NewGuid().ToString();  // Generate new RowKey
            product.ClientId = clientId;  // Set the client ID from session
            Console.WriteLine($"Product initialized with RowKey: {product.RowKey}, ClientId: {product.ClientId}");

            // Clear any errors for RowKey, ClientId, and PartitionKey
            ModelState.Remove(nameof(product.RowKey));
            ModelState.Remove(nameof(product.ClientId));
            ModelState.Remove(nameof(product.PartitionKey));

            if (!ModelState.IsValid)
            {
                Console.WriteLine("Model state is not valid. Errors:");
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    if (state.Errors.Any())
                    {
                        Console.WriteLine($"Key: {key}");
                        var attemptedValue = state.RawValue != null ? state.RawValue.ToString() : "null";
                        Console.WriteLine($"Attempted value: {attemptedValue}");
                        foreach (var error in state.Errors)
                        {
                            Console.WriteLine($"Error: {error.ErrorMessage}");
                        }
                    }
                }
                return View(product);
            }

            // Save the product if the model state is valid
            try
            {
                Console.WriteLine("Model is valid. Attempting to save product...");
                await _productService.AddProductAsync(product);
                Console.WriteLine("Product saved successfully.");
                return RedirectToAction("Dashboard", "Clients");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving product: {ex.Message}");
                _logger.LogError("Error saving product: {0}", ex.Message);
                ModelState.AddModelError("", "An error occurred while saving the product. Please try again.");
                return View(product);
            }
        }



        // GET: Products/Delete
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            Console.WriteLine($"Delete GET called with PartitionKey: {partitionKey}, RowKey: {rowKey}");

            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
            {
                Console.WriteLine("PartitionKey or RowKey is null or empty. Returning NotFound.");
                return NotFound();
            }

            var product = await _productService.GetProductByKeysAsync(partitionKey, rowKey);
            if (product == null)
            {
                Console.WriteLine($"No product found with PartitionKey: {partitionKey}, RowKey: {rowKey}. Returning NotFound.");
                return NotFound();
            }

            Console.WriteLine($"Product found. Displaying delete confirmation for PartitionKey: {partitionKey}, RowKey: {rowKey}.");
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string partitionKey, string rowKey)
        {
            Console.WriteLine($"DeleteConfirmed POST called with PartitionKey: {partitionKey}, RowKey: {rowKey}");

            var product = await _productService.GetProductByKeysAsync(partitionKey, rowKey);
            if (product != null)
            {
                Console.WriteLine($"Product found. Deleting product with PartitionKey: {partitionKey}, RowKey: {rowKey}");
                await _productService.DeleteProductAsync(partitionKey, rowKey);
                Console.WriteLine("Product deletion successful.");
            }
            else
            {
                Console.WriteLine($"No product found with PartitionKey: {partitionKey}, RowKey: {rowKey}. Nothing to delete.");
            }

            return RedirectToAction(nameof(Index));
        }



        private int GetCurrentClientId()
        {
            var clientId = HttpContext.Session.GetInt32("ClientId");
            return clientId ?? -1;
        }






    }




}



