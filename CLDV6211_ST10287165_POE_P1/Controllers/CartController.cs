using CLDV6211_ST10287165_POE_P1.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public class CartController : Controller
{
    private readonly CartService _cartService;
    private readonly ProductService _productService;

    public CartController(CartService cartService, ProductService productService)
    {
        _cartService = cartService;
        _productService = productService;
    }
    [HttpPost]
    public async Task<IActionResult> AddToCart(string productId, int quantity)
    {
        var isLoggedIn = HttpContext.Session.GetString("isLoggedIn");
        var userId = HttpContext.Session.GetString("RowKey");

        if (isLoggedIn != "true")
        {
            return RedirectToAction("Login", "Customers");
        }

        var product = await _productService.GetProductByKeysAsync("Product", productId);
        if (product == null)
        {
            return NotFound("Product not found");
        }

        // `ProductId` now holds the client's ID, and `ProductID` holds the actual product ID
        int totalQuantity = await _cartService.AddOrUpdateItemAsync(userId, productId, product.RowKey, quantity, product.Price, product.Name, product.ImageUrl);

        HttpContext.Session.SetInt32("CartCount", totalQuantity);

        return RedirectToAction("CartView");
    }

    public async Task<IActionResult> CartView()
    {
        var userId = HttpContext.Session.GetString("RowKey");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Customers");
        }

        var cartItems = await _cartService.GetCartItemsAsync(userId);
        var totalQuantity = await _cartService.GetTotalQuantityAsync(userId);
        HttpContext.Session.SetInt32("CartCount", totalQuantity);

        return View(cartItems);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateQuantity(string rowKey, int quantity)
    {
        var userId = HttpContext.Session.GetString("RowKey");
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var cartItem = await _cartService.GetCartItemAsync(userId, rowKey);
        if (cartItem == null)
        {
            return NotFound();
        }

        var product = await _productService.GetProductByKeysAsync("Product", cartItem.ProductID);
        if (product == null)
        {
            return NotFound();
        }

        if (quantity > product.Quantity)
        {
            return Json(new { success = false, message = "Quantity exceeds available stock." });
        }

        cartItem.Quantity = quantity;
        await _cartService.UpdateCartItemAsync(cartItem);

        var cartItems = await _cartService.GetCartItemsAsync(userId);
        var newGrandTotal = cartItems.Sum(i => i.Quantity * i.Price);
        var cartCount = cartItems.Sum(i => i.Quantity);

        HttpContext.Session.SetInt32("CartCount", cartCount);

        return Json(new { success = true, price = cartItem.Price, newGrandTotal, cartCount });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteItem(string rowKey)
    {
        var userId = HttpContext.Session.GetString("RowKey");

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        await _cartService.RemoveItemAsync(userId, rowKey);

        var cartItems = await _cartService.GetCartItemsAsync(userId);
        var newGrandTotal = cartItems.Sum(item => item.Price * item.Quantity);
        var cartCount = cartItems.Sum(item => item.Quantity);
        HttpContext.Session.SetInt32("CartCount", cartCount);

        return Json(new { newGrandTotal, cartCount });
    }
}