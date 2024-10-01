using Microsoft.AspNetCore.Mvc;
using CLDV6211_ST10287165_POE_P1.Models;
using CLDV6211_ST10287165_POE_P1.Services;
using System.Threading.Tasks;
using Azure;
using System;

namespace CLDV6211_ST10287165_POE_P1.Controllers
{
    public class AdminsController : Controller
    {
        private readonly AdminService _adminService;
        private readonly OrderService _orderService;
        public AdminsController(AdminService adminService, OrderService orderService)
        {
            _adminService = adminService;
            _orderService = orderService;

        }

        // GET: Admins
        public async Task<IActionResult> Index()
        {
            var adminID = HttpContext.Session.GetString("AdminRowKey");
            if (string.IsNullOrEmpty(adminID))
            {
                return RedirectToAction("AdminLogin"); // Redirect to login if not logged in
            }

            var admin = await _adminService.GetAdminAsync(adminID);
            if (admin == null)
            {
                return NotFound();
            }

            return View(admin);
        }

        // GET: Admins/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var admin = await _adminService.GetAdminAsync(id);
            if (admin == null)
            {
                return NotFound();
            }

            return View(admin);
        }

        // GET: Admins/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admins/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AdminEmail,AdminPasswordHash")] Admin admin)
        {
            if (ModelState.IsValid)
            {
                admin.PartitionKey = "Admin";
                admin.RowKey = Guid.NewGuid().ToString(); // Generate a unique RowKey (e.g., GUID)

                bool result = await _adminService.AddAdminAsync(admin);

                if (result)
                {
                    return RedirectToAction(nameof(Index));
                }
                ViewBag.ErrorMessage = "Failed to create admin.";
            }
            return View(admin);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            Console.WriteLine($"Edit action called with id (RowKey): {id}");

            // Retrieve the admin by RowKey
            var admin = await _adminService.GetAdminAsync(id);
            if (admin == null)
            {
                Console.WriteLine("Admin not found.");
                return NotFound();
            }

            Console.WriteLine($"Admin to edit: {admin.AdminEmail} with RowKey: {admin.RowKey}");
            return View(admin);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("AdminEmail,AdminPassword,AdminPasswordHash,RowKey,PartitionKey")] Admin admin)
        {
            Console.WriteLine($"Edit POST action called for admin with RowKey: {admin.RowKey}");

            // Ensure that the RowKey from the form matches the RowKey in the URL
            if (id != admin.RowKey)
            {
                Console.WriteLine("RowKey mismatch.");
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Hash the new password if provided
                    if (!string.IsNullOrEmpty(admin.AdminPassword))
                    {
                        admin.AdminPasswordHash = HashPassword(admin.AdminPassword);
                    }
                    else
                    {
                        // Retain the old hash and plain password if no new password is provided
                        var existingAdmin = await _adminService.GetAdminAsync(admin.RowKey);
                        if (existingAdmin != null)
                        {
                            admin.AdminPassword = existingAdmin.AdminPassword;
                            admin.AdminPasswordHash = existingAdmin.AdminPasswordHash;
                        }
                    }

                    // Update the admin in Azure Table Storage
                    await _adminService.UpdateAdminAsync(admin);
                    Console.WriteLine("Admin updated successfully.");
                    return RedirectToAction(nameof(Index));
                }
                catch (RequestFailedException ex)
                {
                    Console.WriteLine($"Error updating admin: {ex.Message}");
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

            return View(admin);
        }

        // POST: Admins/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var admin = await _adminService.GetAdminAsync(id);
            if (admin != null)
            {
                await _adminService.DeleteAdminAsync(admin.PartitionKey, id);
            }

            // Clear session data and redirect to homepage
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // GET: Admin Login
        [HttpGet]
        public IActionResult AdminLogin()
        {
            ViewBag.ShowSignupLink = true; // Assuming the sign-up link is always shown
            return View();
        }

        // POST: Admin Login
        [HttpPost]
        public async Task<IActionResult> AdminLogin(string email, string password)
        {
            var admin = await _adminService.FindAdminByEmailAsync(email);

            // Hash the input password to compare with stored hash
            var hashedInputPassword = HashPassword(password);

            // Check if admin exists and password matches
            if (admin != null && admin.AdminPasswordHash == hashedInputPassword)
            {
                // Store admin details in session variables
                HttpContext.Session.SetString("AdminEmail", admin.AdminEmail);
                HttpContext.Session.SetString("AdminRowKey", admin.RowKey); // Use GUID as RowKey
                HttpContext.Session.SetString("isAdminLoggedIn", "true");

                return RedirectToAction("AdminDashboard");
            }
            else
            {
                ViewBag.ErrorMessage = "Invalid login attempt";
                return View();
            }
        }

        // GET: Admin Sign Up
        [HttpGet]
        public IActionResult AdminSignUp()
        {
            return View();
        }

        // POST: Admin Sign Up
        [HttpPost]
        public async Task<IActionResult> AdminSignUp(string email, string password)
        {
            var adminExists = await _adminService.FindAdminByEmailAsync(email);
            if (adminExists != null)
            {
                ViewBag.ErrorMessage = "User already exists.";
                return View();
            }

            // Hash the password before saving
            var hashedPassword = HashPassword(password);

            // Create a new admin with a GUID RowKey
            var admin = new Admin
            {
                AdminEmail = email,
                AdminPasswordHash = hashedPassword,
                AdminPassword = password,
                PartitionKey = "Admin",
                RowKey = Guid.NewGuid().ToString() // Generate a GUID for RowKey
            };

            bool result = await _adminService.AddAdminAsync(admin);

            if (result)
            {
                return RedirectToAction("AdminLogin");
            }
            else
            {
                ViewBag.ErrorMessage = "Failed to create admin.";
                return View(admin);
            }
        }

        // Admin Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Clears all session data
            return RedirectToAction("Index", "Home"); // Redirect to home page 
        }
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var adminRowKey = HttpContext.Session.GetString("AdminRowKey");
            if (string.IsNullOrEmpty(adminRowKey))
            {
                return RedirectToAction("AdminLogin");
            }

            var admin = await _adminService.GetAdminAsync(adminRowKey);
            if (admin == null)
            {
                Console.WriteLine("Admin not found.");
                return NotFound();
            }

            Console.WriteLine($"Admin to edit: {admin.AdminEmail} with RowKey: {admin.RowKey}");
            return View(admin);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(string id, [Bind("AdminEmail,AdminPassword,AdminPasswordHash,RowKey,PartitionKey")] Admin admin)
        {
            Console.WriteLine($"Profile POST action called for admin with RowKey: {admin.RowKey}");

            // Ensure that the RowKey from the form matches the RowKey in the URL
            if (id != admin.RowKey)
            {
                Console.WriteLine("RowKey mismatch.");
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Hash the new password if provided
                    if (!string.IsNullOrEmpty(admin.AdminPassword))
                    {
                        admin.AdminPasswordHash = HashPassword(admin.AdminPassword); // Hash the password for security
                    }
                    else
                    {
                        // Retain the old values if no new password is provided
                        var existingAdmin = await _adminService.GetAdminAsync(admin.RowKey);
                        if (existingAdmin != null)
                        {
                            admin.AdminPassword = existingAdmin.AdminPassword;
                            admin.AdminPasswordHash = existingAdmin.AdminPasswordHash;
                        }
                    }

                    await _adminService.UpdateAdminAsync(admin);
                    Console.WriteLine("Admin updated successfully.");
                    return RedirectToAction(nameof(Index));
                }
                catch (RequestFailedException ex)
                {
                    Console.WriteLine($"Error updating admin: {ex.Message}");
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

            return View(admin);
        }

        // Helper method to hash passwords
        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }


        // GET: Admin Dashboard
        [HttpGet]
        public IActionResult AdminDashboard()
        {
            return View();
        }
        [HttpGet]


        [HttpGet]
        public IActionResult NewAdmin()
        {
            return View();
        }

        // POST: Admins/NewAdmin
        [HttpPost]
        public async Task<IActionResult> NewAdmin(string email, string password)
        {
            // Check if an admin with the same email already exists using the AdminService
            var adminExists = await _adminService.FindAdminByEmailAsync(email);
            if (adminExists != null)
            {
                ViewBag.ErrorMessage = "User already exists.";
                return View();
            }

            // Hash the password before saving it
            var hashedPassword = HashPassword(password);

            // Create a new admin object with a GUID for RowKey
            var admin = new Admin
            {
                AdminEmail = email,
                AdminPasswordHash = hashedPassword,
                PartitionKey = "Admin", // Static partition key for all admins
                RowKey = Guid.NewGuid().ToString() // Generate a unique RowKey (GUID)
            };

            // Add the new admin using the AdminService
            bool result = await _adminService.AddAdminAsync(admin);

            if (result)
            {
                // Optionally, sign the user in automatically after registration
                // Here, redirect to the admin index or another page as needed
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.ErrorMessage = "Failed to create admin.";
                return View();
            }
        }

        // GET: Order/AllOrders
        public async Task<IActionResult> AllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return View(orders);
        }
        // POST: Order/UpdateOrderStatus
        // Example usage in a controller
        public async Task<IActionResult> UpdateOrderStatus(string orderId, string newStatus)
        {
            if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(newStatus))
            {
                return BadRequest("Invalid order ID or status.");
            }

            try
            {
                // Retrieve the order by RowKey using the new method
                var order = await _orderService.GetOrderByRowKeyAsync(orderId);
                if (order == null)
                {
                    return NotFound("Order not found.");
                }

                // Update the order status
                order.OrderStatus = newStatus;

                // Save the updated order back to Azure Table Storage
                await _orderService.UpdateOrderAsync(order);
                Console.WriteLine($"Order {orderId} updated to status: {newStatus}");

                return RedirectToAction("AllOrders", "Admins");
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine($"Error updating order status: {ex.Message}");
                return StatusCode(500, "Failed to update order status. Please try again.");
            }


        }
    }
}//done 100.0