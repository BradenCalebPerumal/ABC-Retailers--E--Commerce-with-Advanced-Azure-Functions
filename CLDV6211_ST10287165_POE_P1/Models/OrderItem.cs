using Azure;
using Azure.Data.Tables;
using System;

namespace CLDV6211_ST10287165_POE_P1.Models
{
    public class OrderItem : ITableEntity
    {
        public string PartitionKey { get; set; } // This will be the Order RowKey
        public string RowKey { get; set; } // Unique identifier for each order item
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; } // Price per item
        public string CustomerId { get; set; } // RowKey of the customer who placed the order
        public string ClientId { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public OrderItem()
        {
            this.RowKey = Guid.NewGuid().ToString(); // Generates a unique identifier for each order item
        }
    }
}
