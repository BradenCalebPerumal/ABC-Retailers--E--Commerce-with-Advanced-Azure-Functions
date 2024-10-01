using Microsoft.AspNetCore.Mvc;
using CLDV6211_ST10287165_POE_P1.Models;
using CLDV6211_ST10287165_POE_P1.Services;
using System.Threading.Tasks;
using Azure;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;

namespace CLDV6211_ST10287165_POE_P1.Controllers
{
    public class CustomersController : Controller
    {
        private readonly CustomerService _customerService;
        private readonly CartService _cartService;
        private readonly OrderService _orderService;
        private readonly HttpClient _httpClient;

        public CustomersController(CustomerService customerService, CartService cartService, OrderService orderService, HttpClient httpClient)
        {
            _customerService = customerService;
            _cartService = cartService;
            _orderService = orderService;
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return View(customers);
        }

        /*  public async Task<IActionResult> Details(string id)
          {
              var customer = await _customerService.GetCustomerAsync(id);
              if (customer == null)
              {
                  return NotFound();
              }
              return View(customer);
          }*/

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CustEmail,CustPasswordHash")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                customer.PartitionKey = customer.CustEmail; // Use email as PartitionKey for unique identification
                customer.RowKey = Guid.NewGuid().ToString(); // Generate a unique RowKey (e.g., GUID)

                bool result = await _customerService.AddCustomerAsync(customer);

                if (result)
                {
                    return RedirectToAction(nameof(Index));
                }
                ViewBag.ErrorMessage = "Failed to create customer.";
            }
            return View(customer);
        }

        public async Task<IActionResult> Edit(string id)
        {
            Console.WriteLine($"Edit action called with id (RowKey): {id}");

            // Retrieve the customer by RowKey
            var customer = await _customerService.GetCustomerAsync(id);
            if (customer == null)
            {
                Console.WriteLine("Customer not found.");
                return NotFound();
            }

            Console.WriteLine($"Customer to edit: {customer.CustEmail} with RowKey: {customer.RowKey}");
            return View(customer);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("CustEmail,CustPassword,CustPasswordHash,RowKey,PartitionKey")] Customer customer)
        {
            Console.WriteLine($"Edit POST action called for customer with RowKey: {customer.RowKey}");

            // Ensure that the RowKey from the form matches the RowKey in the URL
            if (id != customer.RowKey)
            {
                Console.WriteLine("RowKey mismatch.");
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Hash the new password if provided
                    if (!string.IsNullOrEmpty(customer.CustPassword))
                    {
                        customer.CustPasswordHash = HashPassword(customer.CustPassword);
                    }
                    else
                    {
                        // Retain the old hash if no new password is provided
                        var existingCustomer = await _customerService.GetCustomerAsync(customer.RowKey);
                        if (existingCustomer != null)
                        {
                            customer.CustPasswordHash = existingCustomer.CustPasswordHash;
                        }
                    }

                    // Update the customer in Azure Table Storage
                    await _customerService.UpdateCustomerAsync(customer);
                    Console.WriteLine("Customer updated successfully.");
                    return RedirectToAction(nameof(Index));
                }
                catch (RequestFailedException ex)
                {
                    Console.WriteLine($"Error updating customer: {ex.Message}");
                    throw;
                }
            }
            else
            {
                Console.WriteLine("Model state is not valid.");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"ModelState error: {error.ErrorMessage}");
                }
            }

            return View(customer);
        }


        // GET: Order/History
        public async Task<IActionResult> History()
        {
            // Retrieve the customer's RowKey from the session
            var customerId = HttpContext.Session.GetString("RowKey");
            if (string.IsNullOrEmpty(customerId))
            {
                return RedirectToAction("Login", "Account"); // Redirect to login if session ID is missing
            }

            // Fetch all orders for the customer
            var orders = await _orderService.GetOrdersByCustomerIdAsync(customerId);

            // Create a list to hold orders along with their items
            var orderHistory = new List<OrderHistoryViewModel>();

            // Fetch order items for each order
            foreach (var order in orders)
            {
                var orderItems = await _orderService.GetOrderItemsByOrderIdAsync(order.RowKey);
                orderHistory.Add(new OrderHistoryViewModel
                {
                    Order = order,
                    OrderItems = orderItems
                });
            }

            return View(orderHistory);
        }
        public async Task<IActionResult> Delete(string id)
        {
            var customer = await _customerService.GetCustomerAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var customer = await _customerService.GetCustomerAsync(id);
            if (customer != null)
            {
                await _customerService.DeleteCustomerAsync(customer.PartitionKey, id);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            Console.WriteLine($"Login method called with email: {email}");

            // Retrieve the customer by email
            var customer = await _customerService.FindCustomerByEmailAsync(email);
            if (customer != null)
            {
                // Hash the input password and compare with the stored hash
                var hashedInputPassword = HashPassword(password);

                if (customer.CustPasswordHash == hashedInputPassword)
                {
                    // Store necessary information in the session
                    HttpContext.Session.SetString("CustomerEmail", customer.CustEmail);
                    Console.WriteLine("Login successful.");
                    // During Login
                    if (customer != null && customer.CustPasswordHash == hashedInputPassword)
                    {
                        HttpContext.Session.SetString("CustEmail", customer.CustEmail);
                        HttpContext.Session.SetString("RowKey", customer.RowKey);
                        HttpContext.Session.SetString("isLoggedIn", "true");
                        var cartItems = await _cartService.GetCartItemsAsync(customer.RowKey); // Assuming RowKey is used as UserId
                                                                                               // Calculate the total quantity of all items in the cart

                        int totalQuantity = cartItems.Sum(item => item.Quantity);
                        HttpContext.Session.SetInt32("CartCount", totalQuantity);
                    }

                    // During Logout
                    //  HttpContext.Session.Clear();

                    return RedirectToAction("Index", "Products");
                }
                else
                {
                    Console.WriteLine("Password mismatch.");
                    ViewBag.ErrorMessage = "Invalid login attempt. Please check your credentials.";
                }
            }
            else
            {
                Console.WriteLine("Customer not found.");
                ViewBag.ErrorMessage = "Invalid login attempt. Please check your credentials.";
            }

            return View();
        }
        public async Task<IActionResult> Details(string orderId)
        {
            // Assuming you have a service that can fetch orders and their items
            var order = await _orderService.GetOrderByRowKeyAsync(orderId);
            if (order == null)
                return NotFound("Order not found.");

            // Simulating the fetch of order items - this should also be part of your order service
            var orderItems = await _orderService.GetOrderItemsByOrderIdAsync(orderId);

            // Combine order info and items into a ViewModel
            var viewModel = new OrderHistoryViewModel
            {
                Order = order,
                OrderItems = orderItems
            };

            return PartialView("_OrderDetailsModal", viewModel);
        }



        private static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", string.Empty);
            }
        }

        /* [HttpPost]
         [ValidateAntiForgeryToken]
         public async Task<IActionResult> SignUp(string email, string password)
         {
             Console.WriteLine("SignUp method called");

             if (ModelState.IsValid)
             {
                 Console.WriteLine($"Model is valid. Email: {email}");

                 // Hash the password before storing it
                 var hashedPassword = HashPassword(password);

                 var customer = new Customer
                 {
                     CustEmail = email,
                     CustPassword = password,
                     CustPasswordHash = hashedPassword,
                     PartitionKey = email, // Use email as PartitionKey for unique identification
                     RowKey = email // Use email as RowKey for consistent lookup
                 };

                 bool result = await _customerService.AddCustomerAsync(customer);

                 if (result)
                 {
                     Console.WriteLine("Customer successfully added.");
                     return RedirectToAction("Login");
                 }
                 else
                 {
                     Console.WriteLine("Failed to add customer. Email may already exist.");
                     ViewBag.ErrorMessage = "A user with this email already exists.";
                     return View(customer);
                 }
             }

             Console.WriteLine("Model state is not valid.");
             return View();
         }*/

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(string email, string password)
        {
            Console.WriteLine("SignUp method called");

            // Assuming here that validation like checking empty fields needs to be manually handled
            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
            {
                Console.WriteLine($"Model is valid. Email: {email}");

                // Prepare the JSON payload to send to the Azure Function
                var jsonContent = JsonSerializer.Serialize(new { email, password });
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Replace the URI with your Azure Function's URI
                var response = await _httpClient.PostAsync("https://st10287165bcpfucntion.azurewebsites.net/api/SignUp", content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Customer successfully added.");
                    return RedirectToAction("Login");
                }
                else
                {
                    Console.WriteLine("Failed to add customer. Email may already exist.");
                    ViewBag.ErrorMessage = "A user with this email already exists.";
                    return View(); // Make sure to handle the view properly if expecting a model
                }
            }
            else
            {
                Console.WriteLine("Model state is not valid.");
                ViewBag.ErrorMessage = "Both Email and Password are required.";
                return View(); // Make sure to handle the view properly if expecting a model
            }
        }



        public IActionResult Logoutt()
        {


            HttpContext.Session.Clear(); // Clears the session
            return RedirectToAction("Index", "Home"); // Redirect to home page 
        }
    }
}
/*        public async Task<IActionResult> OrderHistory()
        {
            var userId = HttpContext.Session.GetString("RowKey");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Customers");
            }

            var orders = await _orderService.GetOrdersByCustomerAsync(userId);
            return View(orders);
        }*/
//done