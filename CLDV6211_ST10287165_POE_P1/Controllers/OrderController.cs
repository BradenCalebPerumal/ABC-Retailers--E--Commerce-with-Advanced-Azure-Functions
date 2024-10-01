/*using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CLDV6211_ST10287165_POE_P1.Models;
using CLDV6211_ST10287165_POE_P1.Services;
using Azure.Storage.Queues; // Namespace for Queue storage types
using Newtonsoft.Json;

namespace CLDV6211_ST10287165_POE_P1.Controllers
{
    public class OrderController : Controller
    {
        private readonly OrderService _orderService;
        private readonly CartService _cartService;
        private readonly QueueClient _queueClient;

        public OrderController(OrderService orderService, CartService cartService, QueueClient queueClient)
        {
            _orderService = orderService;
            _cartService = cartService;
            _queueClient = queueClient; // Directly injected
        }



        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            Console.WriteLine("Checkout method started.");

            var userId = HttpContext.Session.GetString("RowKey");
            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("User is not logged in. Redirecting to login page.");
                return RedirectToAction("Login", "Customers");
            }

            Console.WriteLine($"Retrieved User ID from session: {userId}");

            var cartItems = await _cartService.GetCartItemsAsync(userId);
            if (cartItems.Count == 0)
            {
                Console.WriteLine("No items in cart. Redirecting to cart view.");
                return RedirectToAction("CartView", "Cart");
            }

            var model = new CheckoutViewModel
            {
                FullName = HttpContext.Session.GetString("CustomerEmail"),
                CartItems = cartItems,
                TotalAmount = cartItems.Sum(i => i.Price * i.Quantity),
                PaymentMethod = "Cash on Delivery"
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            Console.WriteLine("Checkout POST method started.");

            var userId = HttpContext.Session.GetString("RowKey");
            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("User is not logged in. Redirecting to login page.");
                return RedirectToAction("Login", "Customers");
            }

            Console.WriteLine($"Retrieved User ID from session: {userId}");
            // Remove fields from validation
            ModelState.Remove("RowKey");
            ModelState.Remove("PartitionKey");
            ModelState.Remove("CartItems");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is not valid.");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"ModelState Error: {error.ErrorMessage}");
                }
                return View(model);
            }

            // Create a new order
            var order = new Order
            {
                PartitionKey = userId,
                RowKey = Guid.NewGuid().ToString(),
                ShippingAddress = model.ShippingAddress,
                City = model.City,
                PostalCode = model.PostalCode,
                Country = model.Country,
                PaymentMethod = model.PaymentMethod,
                OrderDate = DateTime.UtcNow,
                OrderStatus = "Pending"
            };

            // Create order details
            var orderDetails = model.CartItems.Select(item => new OrderDetail
            {
                PartitionKey = order.RowKey,
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                Price = item.Price,
                ProductImageUrl = item.ProductImageUrl
            }).ToList();

            // Save the order and order details
            await _orderService.PlaceOrderAsync(order, orderDetails);
            Console.WriteLine("Order and order details saved successfully.");

            // Clear the cart after successful order
            await _cartService.ClearCartAsync(userId);
            HttpContext.Session.SetInt32("CartCount", 0);
            Console.WriteLine("Cart cleared successfully.");

            // Create the queue message for order processing
            var orderQueueMessage = new OrderQueueMessage
            {
                OrderId = order.RowKey,
                CustomerId = userId,
                Status = order.OrderStatus
            };

            // Send the order message to the queue
            var message = JsonConvert.SerializeObject(orderQueueMessage);
            await _queueClient.SendMessageAsync(message);
            Console.WriteLine("Order message sent to the queue successfully.");

            // Redirect to the order confirmation page
            return RedirectToAction("OrderConfirmation", new { orderId = order.RowKey });
        }
        public async Task<IActionResult> OrderConfirmation(string orderId)
        {
            var userId = HttpContext.Session.GetString("RowKey");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Customers");
            }

            var order = await _orderService.GetOrderAsync(userId, orderId);
            if (order == null)
            {
                return NotFound();
            }

            var orderDetails = await _orderService.GetOrderDetailsAsync(orderId);
            var model = new OrderConfirmationViewModel
            {
                OrderId = orderId,
                OrderStatus = order.OrderStatus,
                ShippingAddress = order.ShippingAddress,
                City = order.City,
                PostalCode = order.PostalCode,
                Country = order.Country,
                PaymentMethod = order.PaymentMethod,
                OrderDate = order.OrderDate,
                OrderDetails = orderDetails
            };

            return View(model);
        }
    }
}
*/

using CLDV6211_ST10287165_POE_P1.Models;
using CLDV6211_ST10287165_POE_P1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Azure;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;

namespace CLDV6211_ST10287165_POE_P1.Controllers
{
    public class OrderController : Controller
    {
        private readonly OrderService _orderService;
        private readonly CartService _cartService;
        private readonly CustomerService _customerTableClient;
        private readonly HttpClient _httpClient;

        public OrderController(OrderService orderService, CartService cartservice, CustomerService customerTableClient, HttpClient _httpClient)
        {
            _orderService = orderService;
            _cartService = cartservice;
            _customerTableClient = customerTableClient;
            _customerTableClient = customerTableClient;
        }
        public async Task<IActionResult> Index()
        {
            // Retrieve all orders using the service
            var orders = await _orderService.GetAllOrdersAsync();

            // Since emails are now stored in the Orders table, no need to fetch separately
            // Directly pass the orders to the view
            return View(orders);
        }

        public async Task<IActionResult> Checkout()
        {
            var userId = HttpContext.Session.GetString("RowKey");
            Console.WriteLine($"User RowKey retrieved from session: {userId}");

            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("User is not logged in. Redirecting to login.");
                return RedirectToAction("Login", "Account");
            }

            var cartItems = await _cartService.GetCartItemsAsync(userId);
            Console.WriteLine($"Number of items in cart: {cartItems?.Count ?? 0}");

            if (cartItems == null || !cartItems.Any())
            {
                Console.WriteLine("Cart is empty. Redirecting to CartView.");
                return RedirectToAction("CartView", "Cart");
            }

            var totalAmount = cartItems.Sum(item => item.Price * item.Quantity);
            Console.WriteLine($"Calculated Total Amount: {totalAmount:C}");
            // Retrieve the customer's email using the userId (RowKey)
            string customerEmail = null;
            try
            {
                var customer = await _customerTableClient.GetCustomerAsync(userId); // Fetching the customer by RowKey
                if (customer != null)
                {
                    customerEmail = customer.CustEmail; // Assuming CustEmail is the email property in the Customer model
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to retrieve customer email: {ex.Message}");
                // Handle exception (e.g., log the error, set a default value, etc.)
            }
            var viewModel = new CheckoutViewModel
            {
                CartItems = cartItems,
                Order = new Order
                {
                    PartitionKey = userId, // Set the PartitionKey to the user's RowKey
                    TotalAmount = totalAmount,
                    CustEmail = customerEmail ?? "default@example.com",
                    ShippingAddress = "", // Initialized to empty, user will input this
                    OrderDate = DateTime.UtcNow,
                    PaymentMethod = "COD", // Example value, can be changed
                    OrderStatus = ""
                }
            };



            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            // Log entry into the method
            Console.WriteLine("Entering PlaceOrder method.");

            // Validate the incoming model
            if (model == null || model.Order == null || model.CartItems == null || !model.CartItems.Any())
            {
                Console.WriteLine("Error: Submission model is null or incomplete.");
                return RedirectToAction("Checkout"); // Redirect user back to checkout page to correct the submission
            }

            // Ensure the PartitionKey is set correctly
            if (string.IsNullOrEmpty(model.Order.PartitionKey))
            {
                Console.WriteLine("Error: Order PartitionKey is missing.");
                return RedirectToAction("Error", new { message = "Order processing error: Missing user identifier." });
            }

            // Process the order
            try
            {
                // Create the order in the database
                await _orderService.CreateOrderAsync(model.Order);
                Console.WriteLine($"Order created successfully with PartitionKey = {model.Order.PartitionKey} and RowKey = {model.Order.RowKey}.");
                HttpContext.Session.SetString("OrderRowKey", model.Order.RowKey);
                // Create each order item
                foreach (var cartItem in model.CartItems)
                {
                    var orderItem = new OrderItem
                    {
                        PartitionKey = model.Order.RowKey, // Link order items to the order using the order's RowKey
                        ProductId = cartItem.ProductId,
                        ProductName = cartItem.ProductName,

                        Quantity = cartItem.Quantity,
                        Price = cartItem.Price,
                        CustomerId = model.Order.PartitionKey // Link the order item to the customer using the order's PartitionKey
                    };

                    // Add the order item to the database
                    await _orderService.CreateOrderItemAsync(orderItem);


                    Console.WriteLine($"Order item for {orderItem.ProductName} created successfully.");
                }

                // Redirect to an order confirmation page, passing the OrderId as a query parameter
                return RedirectToAction("OrderConfirmation", new { orderId = model.Order.RowKey });
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"An error occurred while creating the order: {ex.Message}");
                return RedirectToAction("Error", new { message = "Order processing failed. Please try again." });
            }
        }

        // GET: Order/Confirmation/{rowKey}
        public async Task<IActionResult> OrderConfirmation()
        {
            // Retrieve OrderRowKey from the session
            var orderRowKey = HttpContext.Session.GetString("OrderRowKey");

            // Check if the OrderRowKey is available in the session
            if (string.IsNullOrEmpty(orderRowKey))
            {
                // If the session doesn't contain the OrderRowKey, redirect to an error page or handle accordingly
                return View("Error", "Order not found or session expired.");
            }

            // Fetch the order confirmation message from the queue using the OrderRowKey
            var message = await _orderService.GetOrderConfirmationMessageAsync(orderRowKey);

            // Pass the retrieved message to the view
            ViewBag.OrderMessage = message;
            return View();
        }

        // GET: Order/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                // Retrieve the order by RowKey
                var order = await _orderService.GetOrderAsync(id);
                if (order == null)
                {
                    return NotFound();
                }

                return View(order);
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine($"Error retrieving order: {ex.Message}");
                return RedirectToAction("Error", new { message = "Failed to load the order. Please try again." });
            }
        }

        // POST: Order/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("RowKey,PartitionKey,OrderStatus")] Order order)
        {
            if (id != order.RowKey)
            {
                Console.WriteLine("RowKey mismatch.");
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Retrieve the existing order to update only the status
                    var existingOrder = await _orderService.GetOrderAsync(order.RowKey);
                    if (existingOrder == null)
                    {
                        Console.WriteLine("Order not found.");
                        return NotFound();
                    }

                    // Update only the status field
                    existingOrder.OrderStatus = order.OrderStatus;

                    // Save changes to the database
                    await _orderService.UpdateOrderAsync(existingOrder);
                    Console.WriteLine($"Order updated successfully: {existingOrder.RowKey}");

                    return RedirectToAction(nameof(Index)); // Redirect to the orders list
                }
                catch (RequestFailedException ex)
                {
                    Console.WriteLine($"Error updating order: {ex.Message}");
                    return RedirectToAction("Error", new { message = "Failed to update the order. Please try again." });
                }
            }

            Console.WriteLine("Model state is not valid.");
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine($"ModelState error: {error.ErrorMessage}");
            }

            return View(order); // Return the view with the current model to show validation errors
        }

    }
}
//DONE :)