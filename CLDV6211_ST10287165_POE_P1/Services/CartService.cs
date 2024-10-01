using Azure.Data.Tables;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CLDV6211_ST10287165_POE_P1.Models;
using Azure; // Ensure this matches your actual namespace

public class CartService
{
    private readonly TableClient _tableClient;

    public CartService(string connectionString)
    {
        _tableClient = new TableClient(connectionString, "CartItems");
        _tableClient.CreateIfNotExists();
    }

    public async Task<bool> UpdateItemQuantityAsync(string userId, string rowKey, int quantity)
    {
        try
        {
            var cartItem = await _tableClient.GetEntityAsync<CartItem>(userId, rowKey);
            if (cartItem != null)
            {
                cartItem.Value.Quantity = quantity;
                await _tableClient.UpdateEntityAsync(cartItem.Value, ETag.All, TableUpdateMode.Replace);
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating item quantity: {ex.Message}");
        }

        return false;
    }

    public async Task<int> AddOrUpdateItemAsync(string userId, string productId, string productID, int quantity, double price, string productName, string productImageUrl)
    {
        // ProductID now references the actual product in the cart
        var existingItem = (await GetCartItemsAsync(userId)).FirstOrDefault(item => item.ProductID == productID);
        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
            await _tableClient.UpdateEntityAsync(existingItem, existingItem.ETag, TableUpdateMode.Replace);
        }
        else
        {
            var cartItem = new CartItem
            {
                PartitionKey = userId,
                RowKey = Guid.NewGuid().ToString(),
                ProductId = productId, // This will be the client's ID
                ProductID = productID, // This will be the actual product ID
                Quantity = quantity,
                Price = price,
                ProductName = productName,
                ProductImageUrl = productImageUrl
            };
            await _tableClient.AddEntityAsync(cartItem);
        }

        // Calculate the total quantity in the cart
        var cartItems = await GetCartItemsAsync(userId);
        int totalQuantity = cartItems.Sum(item => item.Quantity);

        return totalQuantity;
    }

    public async Task<int> GetTotalQuantityAsync(string userId)
    {
        var items = await GetCartItemsAsync(userId);
        return items.Sum(item => item.Quantity);
    }

    public async Task<List<CartItem>> GetCartItemsAsync(string userId)
    {
        var items = new List<CartItem>();
        try
        {
            // Query items based on the PartitionKey, which is set to userId
            var query = _tableClient.QueryAsync<CartItem>(filter: $"PartitionKey eq '{userId}'");
            await foreach (var item in query)
            {
                items.Add(item);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving cart items: {ex.Message}");
            throw;
        }
        return items;
    }

    public async Task RemoveItemAsync(string userId, string rowKey)
    {
        await _tableClient.DeleteEntityAsync(userId, rowKey);
    }

    public async Task<CartItem> GetCartItemAsync(string userId, string rowKey)
    {
        try
        {
            var cartItem = await _tableClient.GetEntityAsync<CartItem>(userId, rowKey);
            return cartItem.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task UpdateCartItemAsync(CartItem cartItem)
    {
        try
        {
            await _tableClient.UpdateEntityAsync(cartItem, ETag.All, TableUpdateMode.Replace);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating cart item: {ex.Message}");
            throw;
        }
    }

    public async Task ClearCartAsync(string userId)
    {
        var cartItems = await GetCartItemsAsync(userId);
        foreach (var item in cartItems)
        {
            await _tableClient.DeleteEntityAsync(item.PartitionKey, item.RowKey);
        }
    }
}