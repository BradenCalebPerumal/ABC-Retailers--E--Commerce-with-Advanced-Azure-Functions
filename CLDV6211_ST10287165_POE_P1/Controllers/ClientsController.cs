using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using CLDV6211_ST10287165_POE_P1.Models;
using CLDV6211_ST10287165_POE_P1.Services;
using System.Text;
using System.Text.Json;

namespace CLDV6211_ST10287165_POE_P1.Controllers
{
    public class ClientsController : Controller
    {
        private readonly ClientService _clientService;
        private readonly ProductService _productService;
        private readonly ILogger<ClientsController> _logger;
        private readonly FileShareService _fileShareService;
        private readonly OrderService _orderService;
        public ClientsController(ClientService clientService, ProductService productService, ILogger<ClientsController> logger, FileShareService fileShareServices, OrderService orderService)
        {
            _clientService = clientService;
            _productService = productService;
            _logger = logger;
            _fileShareService = fileShareServices;
            _orderService = orderService;
        }

        // GET: Clients
        public async Task<IActionResult> Index()
        {
            var clients = await _clientService.GetAllClientsAsync();
            return View(clients);
        }
        public async Task<IActionResult> ClientDetails()
        {
            return View();
        }

        // GET: Clients/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var client = await _clientService.GetClientByIdAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // GET: Clients/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Client client)
        {
            if (ModelState.IsValid)
            {
                await _clientService.AddClientAsync(client);
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        /*    // GET: Clients/Edit/5
            public async Task<IActionResult> Edit(string id)
            {
                if (string.IsNullOrEmpty(id))
                {
                    return NotFound();
                }

                var client = await _clientService.GetClientByIdAsync(id);
                if (client == null)
                {
                    return NotFound();
                }
                return View(client);
            }

            // POST: Clients/Edit/5
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(string id, Client client)
            {
                if (id != client.RowKey)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        await _clientService.UpdateClientAsync(client);
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating client.");
                        throw;
                    }
                }
                return View(client);
            }*/

        // GET: Clients/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var client = await _clientService.GetClientByIdAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // POST: Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _clientService.DeleteClientAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Loginn()
        {
            return View();
        }

        [HttpGet]
        public IActionResult SIgnUp()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SIgnUp(Client model, IFormFile contractFile, bool agreeTerms)
        {
            Console.WriteLine("SignUp method called.");

            if (!agreeTerms)
            {
                Console.WriteLine("User did not agree to the terms.");
                ViewBag.ErrorMessage = "You must agree to the terms.";
                return View(model);
            }

            var userExists = await _clientService.FindClientByUsernameAsync(model.Username);
            if (userExists != null)
            {
                Console.WriteLine("Client already exists.");
                ViewBag.ErrorMessage = "Client already exists.";
                return View(model);
            }

            // Generate and upload auto-generated PDF contract
            Console.WriteLine("Generating PDF contract...");
            byte[] pdfFile = _fileShareService.GenerateContractPdf(model);
            var autoGeneratedFileName = $"autogeneratedcontracts/{model.Username}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";

            using (var stream = new MemoryStream(pdfFile))
            {
                Console.WriteLine("Uploading auto-generated PDF contract...");
                if (!await _fileShareService.UploadFileAsync(autoGeneratedFileName, stream))
                {
                    Console.WriteLine("Failed to upload auto-generated contract.");
                    ViewBag.ErrorMessage = "Failed to upload auto-generated contract.";
                    return View(model);
                }
            }
            if (contractFile != null && contractFile.Length > 0)
            {
                // Ensuring the file name is unique with a timestamp and username
                var fileName = $"contracts/{model.Username}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
                Console.WriteLine($"Processing file upload for: {fileName}");

                using (var stream = contractFile.OpenReadStream())
                {
                    stream.Position = 0;  // Reset the position of the stream to the beginning
                    if (stream.Length > 0)
                    {
                        var uploadResult = await _fileShareService.UploadFileAsync(fileName, stream);
                        Console.WriteLine($"File upload result: {uploadResult}");
                        if (!uploadResult)
                        {
                            ViewBag.ErrorMessage = "File upload failed.";
                            return View(model);
                        }
                    }
                    else
                    {
                        Console.WriteLine("File stream is empty.");
                        ViewBag.ErrorMessage = "Uploaded file is empty.";
                        return View(model);
                    }
                }
            }
            else
            {
                Console.WriteLine("No file uploaded.");
            }

            await _clientService.AddClientAsync(model);
            Console.WriteLine("Client added successfully, redirecting to login.");
            return RedirectToAction("Loginn");
        }



        public async Task<IActionResult> SIignUp(Client model, IFormFile contractFile, bool agreeTerms)
        {
            Console.WriteLine("SignUp action called.");
            if (!agreeTerms)
            {
                Console.WriteLine("User did not agree to the terms.");
                ViewBag.ErrorMessage = "You must agree to the terms.";
                return View(model);
            }

            var userExists = await _clientService.FindClientByUsernameAsync(model.Username);
            if (userExists != null)
            {
                Console.WriteLine("Client already exists.");
                ViewBag.ErrorMessage = "Client already exists.";
                return View(model);
            }

            Console.WriteLine("Generating PDF contract...");
            byte[] pdfFile = _fileShareService.GenerateContractPdf(model);
            var autoGeneratedFileName = $"autogeneratedcontracts/{model.Username}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";

            try
            {
                using (var stream = new MemoryStream(pdfFile))
                {
                    Console.WriteLine("Uploading auto-generated PDF contract...");
                    bool uploadResult = await _fileShareService.UploadFileToFunction(autoGeneratedFileName, stream);
                    Console.WriteLine("Auto-generated contract upload result: " + uploadResult);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error uploading auto-generated contract: " + ex.Message);
                ViewBag.ErrorMessage = "Failed to upload auto-generated contract.";
                return View(model);
            }

            if (contractFile != null && contractFile.Length > 0)
            {
                var fileName = $"contracts/{model.Username}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
                Console.WriteLine($"Processing file upload for: {fileName}");
                try
                {
                    using (var stream = contractFile.OpenReadStream())
                    {
                        bool uploadResult = await _fileShareService.UploadFileToFunction(fileName, stream);
                        Console.WriteLine("User contract file upload result: " + uploadResult);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error uploading client's contract file: " + ex.Message);
                    ViewBag.ErrorMessage = "File upload failed.";
                    return View(model);
                }
            }
            else
            {
                Console.WriteLine("No contract file provided by the user.");
            }

            try
            {
                await _clientService.AddClientAsync(model);
                Console.WriteLine("Client added successfully, redirecting to login.");
                return RedirectToAction("Loginn");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error adding client to database: " + ex.Message);
                ViewBag.ErrorMessage = "Error saving client details.";
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Loginn(string email, string password)
        {
            Console.WriteLine("Loginn method called");
            Console.WriteLine($"Email provided: {email}");

            var client = await _clientService.FindClientByUsernameAsync(email);

            if (client == null)
            {
                Console.WriteLine("No client found with the provided email.");
                ViewBag.ErrorMessage = "Invalid login attempt";
                return View();
            }

            Console.WriteLine($"Client found: {client.Username}, checking password...");

            if (client.Password != password)
            {
                Console.WriteLine("Password mismatch.");
                ViewBag.ErrorMessage = "Invalid login attempt";
                return View();
            }

            Console.WriteLine("Login successful, setting session variables.");
            HttpContext.Session.SetString("IsClientLoggedIn", "true");
            HttpContext.Session.SetString("ClientId", client.RowKey);

            return RedirectToAction("Dashboard");
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Dashboard()
        {
            var clientId = HttpContext.Session.GetString("ClientId");

            if (!string.IsNullOrEmpty(clientId))
            {
                var client = await _clientService.GetClientByIdAsync(clientId);

                if (client != null)
                {
                    ViewBag.FullName = $"{client.ClientFirstName} {client.LastName}";
                }
                else
                {
                    ViewBag.FullName = "Unknown";
                }
            }
            else
            {
                ViewBag.FullName = "Unknown";
            }

            return View();
        }

        public async Task<IActionResult> ListedProducts()
        {
            var isClientLoggedIn = HttpContext.Session.GetString("IsClientLoggedIn");
            if (isClientLoggedIn != "true")
            {
                return RedirectToAction("Loginn");
            }

            var clientId = HttpContext.Session.GetString("ClientId");
            var clientProducts = await _productService.GetProductsByClientIdAsync(clientId);

            if (clientProducts == null || !clientProducts.Any())
            {
                ViewBag.Message = "No products listed.";
            }

            return View(clientProducts);
        }

        public async Task<IActionResult> EditProducts()
        {
            var isClientLoggedIn = HttpContext.Session.GetString("IsClientLoggedIn");
            if (isClientLoggedIn != "true")
            {
                return RedirectToAction("Loginn");

            }

            var clientId = HttpContext.Session.GetString("ClientId");
            var clientProducts = await _productService.GetProductsByClientIdAsync(clientId);

            return View(clientProducts);
        }

        public async Task<IActionResult> EditProduct(string id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(Product product)
        {
            if (ModelState.IsValid)
            {
                await _productService.UpdateProductAsync(product);
                return RedirectToAction("EditProducts");
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(string id, string confirmation)
        {
            if (confirmation == "delete")
            {
                await _productService.DeleteProductAsync(id);
            }

            return RedirectToAction("EditProducts");
        }

        public async Task<IActionResult> ClientOrders()
        {
            // Retrieve the client's RowKey from the session
            var clientId = HttpContext.Session.GetString("ClientId");
            Console.WriteLine($"Client RowKey retrieved from session: {clientId}");

            if (string.IsNullOrEmpty(clientId))
            {
                Console.WriteLine("Client is not logged in. Redirecting to login.");
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Query OrderItem table to find all items with ProductId matching the client's RowKey
                var orderItems = await _orderService.GetOrderItemsByClientAsync(clientId);

                // Group order items by Order PartitionKey (linking to Order table)
                var groupedOrderItems = orderItems
                    .GroupBy(item => item.PartitionKey)
                    .Select(group => new
                    {
                        OrderId = group.Key, // This is the PartitionKey in Order table
                        Products = group.Select(item => new
                        {
                            item.ProductName,
                            item.Quantity,
                            item.Price
                        }).ToList()
                    }).ToList();

                // Prepare a view model to display the ordered products and quantities
                var viewModel = new ClientOrdersViewModel
                {
                    ClientId = clientId,
                    Orders = groupedOrderItems.Select(order => new OrderViewModel
                    {
                        OrderId = order.OrderId,
                        Products = order.Products.Select(p => new ProductViewModel
                        {
                            Name = p.ProductName,
                            Quantity = p.Quantity,
                            TotalPrice = p.Price * p.Quantity
                        }).ToList()
                    }).ToList()
                };

                // Return the view with the populated view model
                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving client orders: {ex.Message}");
                return RedirectToAction("Error", new { message = "Failed to retrieve client orders. Please try again." });
            }
        }
        // GET: Client/Edit
        [HttpGet]
        [Route("Client/Edit")] // Specific route to distinguish this action
        public async Task<IActionResult> Edit()
        {
            var clientId = HttpContext.Session.GetString("ClientId");
            Console.WriteLine($"Client RowKey retrieved from session: {clientId}");

            if (string.IsNullOrEmpty(clientId))
            {
                return RedirectToAction("Login", "Account");
            }

            var client = await _clientService.GetClientByIdAsync(clientId);
            if (client == null)
            {
                return NotFound("Client not found.");
            }

            return View(client);
        }

        // POST: Client/Edit
        [HttpPost]
        [Route("Client/Edit")] // Same route but distinguished by HTTP method
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Client client)
        {
            var clientId = HttpContext.Session.GetString("ClientId");

            if (string.IsNullOrEmpty(clientId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(client);
            }

            try
            {
                var existingClient = await _clientService.GetClientByIdAsync(clientId);
                if (existingClient == null)
                {
                    return NotFound("Client not found.");
                }

                existingClient.Password = client.Password;
                existingClient.CellNum = client.CellNum;

                await _clientService.UpdateClientAsync(existingClient);

                TempData["SuccessMessage"] = "Client details updated successfully!";
                return RedirectToAction("Dashboard");
            }
            catch
            {
                ModelState.AddModelError("", "An error occurred while updating the client. Please try again.");
                return View(client);
            }



        }
        public async Task<IActionResult> ClientDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound("Client ID is required.");
            }

            // Fetch the client details by RowKey
            var client = await _clientService.GetClientByIdAsync(id);
            if (client == null)
            {
                return NotFound("Client not found.");
            }

            // Return the client details to the view
            return View(client);
        }
    }
}
//done