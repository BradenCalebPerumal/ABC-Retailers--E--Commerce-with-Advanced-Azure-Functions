/*using Microsoft.AspNetCore.Mvc;
using CLDV6211_ST10287165_POE_P1.Models;
using CLDV6211_ST10287165_POE_P1.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CLDV6211_ST10287165_POE_P1.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly CartService _cartService;
        private readonly OrderService _orderService;
        private readonly QueueService _queueService;  // Service for handling Azure Queue messages

        public CheckoutController(CartService cartService, OrderService orderService, QueueService queueService)
        {
            _cartService = cartService;
            _orderService = orderService;
            _queueService = queueService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("RowKey");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Customers");
            }

            var cartItems = await _cartService.GetCartItemsAsync(userId);
            if (cartItems == null || !cartItems.Any())
            {
                return RedirectToAction("CartView", "Cart");
            }

            var viewModel = new CheckoutViewModel
            {
                FullName = string.Empty, // Optional: Pre-fill with user data if available
                ShippingAddress = string.Empty,
                City = string.Empty,
                PostalCode = string.Empty,
                Country = string.Empty,
                PaymentMethod = "Cash on Delivery",
                CartItems = cartItems,
                TotalAmount = cartItems.Sum(item => item.Quantity * item.Price)
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Index(CheckoutViewModel viewModel)
        {
            var userId = HttpContext.Session.GetString("RowKey");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Customers");
            }

            if (!ModelState.IsValid)
            {
                var cartItems = await _cartService.GetCartItemsAsync(userId);
                viewModel.CartItems = cartItems;
                viewModel.TotalAmount = cartItems.Sum(item => item.Quantity * item.Price);
                return View(viewModel);
            }

            // Create Order
            var order = new Order
            {
                PartitionKey = userId,
                RowKey = Guid.NewGuid().ToString(),
                OrderDate = DateTime.UtcNow,
                OrderStatus = "Pending",
                ShippingAddress = viewModel.ShippingAddress,
                City = viewModel.City,
                PostalCode = viewModel.PostalCode,
                Country = viewModel.Country,
                PaymentMethod = viewModel.PaymentMethod
            };

            var orderDetails = viewModel.CartItems.Select(item => new OrderDetail
            {
                PartitionKey = order.RowKey,
                RowKey = Guid.NewGuid().ToString(),
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = item.Price,
                ProductName = item.ProductName,
                ProductImageUrl = item.ProductImageUrl
            }).ToList();

            // Place the order
            await _orderService.PlaceOrderAsync(order, orderDetails);

            // Clear the cart after successful order
            await _cartService.ClearCartAsync(userId);

            // Create the order queue message
            var orderQueueMessage = new OrderQueueMessage
            {
                OrderId = order.RowKey,
                CustomerId = userId,
                Status = order.OrderStatus
            };

            // Send the message to the Azure Queue
            await _queueService.SendMessageAsync(orderQueueMessage);

            // Redirect to order confirmation page
            return RedirectToAction("OrderConfirmation", new { orderId = order.RowKey });
        }

        [HttpGet]
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

            var orderDetails = await _orderService.GetOrderDetailsAsync(order.RowKey);
            
           
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