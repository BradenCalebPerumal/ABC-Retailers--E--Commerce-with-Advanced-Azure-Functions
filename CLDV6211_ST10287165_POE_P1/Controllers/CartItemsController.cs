/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CLDV6211_ST10287165_POE_P1.Data;
using CLDV6211_ST10287165_POE_P1.Models;

namespace CLDV6211_ST10287165_POE_P1.Controllers
{
    public class CartItemsController : Controller
    {
        private readonly CLDV6211_ST10287165_POE_P1Context _context;

        public CartItemsController(CLDV6211_ST10287165_POE_P1Context context)
        {
            _context = context;
        }

        // GET: CartItems
        public async Task<IActionResult> Index()
        {
            var cLDV6211_ST10287165_POE_P1Context = _context.CartItems.Include(c => c.Customer).Include(c => c.Product);
            return View(await cLDV6211_ST10287165_POE_P1Context.ToListAsync());
        }

        // GET: CartItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cartItem = await _context.CartItems
                .Include(c => c.Customer)
                .Include(c => c.Product)
                .FirstOrDefaultAsync(m => m.CartItemId == id);
            if (cartItem == null)
            {
                return NotFound();
            }

            return View(cartItem);
        }

        // GET: CartItems/Create
        public IActionResult Create()
        {
            ViewData["CustId"] = new SelectList(_context.Customer, "CustId", "CustId");
            ViewData["ProductId"] = new SelectList(_context.Product, "Id", "ImageType");
            return View();
        }

        // POST: CartItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CartItemId,ProductId,Quantity,CustId")] CartItem cartItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cartItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustId"] = new SelectList(_context.Customer, "CustId", "CustId", cartItem.CustId);
            ViewData["ProductId"] = new SelectList(_context.Product, "Id", "ImageType", cartItem.ProductId);
            return View(cartItem);
        }

        // GET: CartItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem == null)
            {
                return NotFound();
            }
            ViewData["CustId"] = new SelectList(_context.Customer, "CustId", "CustId", cartItem.CustId);
            ViewData["ProductId"] = new SelectList(_context.Product, "Id", "ImageType", cartItem.ProductId);
            return View(cartItem);
        }

        // POST: CartItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CartItemId,ProductId,Quantity,CustId")] CartItem cartItem)
        {
            if (id != cartItem.CartItemId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cartItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CartItemExists(cartItem.CartItemId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustId"] = new SelectList(_context.Customer, "CustId", "CustId", cartItem.CustId);
            ViewData["ProductId"] = new SelectList(_context.Product, "Id", "ImageType", cartItem.ProductId);
            return View(cartItem);
        }

        // GET: CartItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cartItem = await _context.CartItems
                .Include(c => c.Customer)
                .Include(c => c.Product)
                .FirstOrDefaultAsync(m => m.CartItemId == id);
            if (cartItem == null)
            {
                return NotFound();
            }

            return View(cartItem);
        }

       
        private bool CartItemExists(int id)
        {
            return _context.CartItems.Any(e => e.CartItemId == id);
        }

       

        public IActionResult AddToCart(int productId, int quantity)
        {
            // Check if the session contains a logged-in customer ID and retrieve it safely
            if (!HttpContext.Session.GetInt32("CustId").HasValue)
            {
                TempData["Error"] = "Please log in to add items to your cart.";
                return RedirectToAction("Login", "Customers");
            }

            int custId = HttpContext.Session.GetInt32("CustId").Value;  // This is now safe to access

            var product = _context.Product.Find(productId);
            if (product == null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction("Index", "Products");
            }

            var cartItem = _context.CartItems.FirstOrDefault(c => c.ProductId == productId && c.CustId == custId);
            if (cartItem == null)
            {
                _context.CartItems.Add(new CartItem { ProductId = productId, Quantity = quantity, CustId = custId });
            }
            else
            {
                cartItem.Quantity += quantity;
            }

            _context.SaveChanges();

            // Update the session cart count after adding to the cart
            UpdateCartCount(custId);

            return RedirectToAction("CartView");  // Redirect to the cart view
        }

        private void UpdateCartCount(int custId)
        {
            var newCartCount = _context.CartItems.Where(c => c.CustId == custId).Sum(c => c.Quantity);
            HttpContext.Session.SetInt32("CartCount", newCartCount);
        }
        public IActionResult CartView()
        {
            if (HttpContext.Session.GetString("IsLoggedIn") != "true")
            {
                TempData["Error"] = "Please log in to view your cart.";
                return RedirectToAction("Login", "Customers");
            }

            var custId = HttpContext.Session.GetInt32("CustId").Value;  // Assumes CustId is always set if IsLoggedIn is true
            var cartItems = _context.CartItems.Include(c => c.Product).Where(c => c.CustId == custId).ToList();

            return View(cartItems);  // Return the view with the cart items
        }
        [HttpPost]
        public IActionResult RemoveFromCart(int cartItemId)
        {
            var cartItem = _context.CartItems.Find(cartItemId);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                _context.SaveChanges();

                // Update cart count in session
                UpdateCartCountInSession();
            }

            return RedirectToAction("CartView");  // Redirects to the cart view to show updated cart
        }
        private void UpdateCartCountInSession()
        {
            try
            {
                // Check if the customer ID is available in the session.
                if (HttpContext.Session.GetInt32("CustId") is int custId)
                {
                    // Retrieve all cart items for the logged-in customer and calculate the sum of the quantities.
                    var cartCount = _context.CartItems.Where(c => c.CustId == custId).Sum(c => c.Quantity);

                    // Update the session with the new cart count.
                    HttpContext.Session.SetInt32("CartCount", cartCount);
                }
                else
                {
                    // If there is no customer ID in the session, remove the cart count from the session.
                    HttpContext.Session.Remove("CartCount");
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it according to your logging strategy.
                // You might want to consider how critical this method is for your application's functionality
                // and handle the exception appropriately (e.g., by setting the session variable to 0 or sending an error response).
                Console.WriteLine("Error updating cart count: " + ex.Message);
                // Optionally reset the cart count in the session to 0 to avoid showing stale data
                HttpContext.Session.SetInt32("CartCount", 0);
            }
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();

                // Update the session cart count
                UpdateCartCount(cartItem.CustId);

                return RedirectToAction(nameof(CartView));
            }
            return NotFound();
        }
    }
}
*/