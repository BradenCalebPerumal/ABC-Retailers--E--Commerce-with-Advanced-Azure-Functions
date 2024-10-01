using Azure;
using Azure.Data.Tables;
using System;

namespace CLDV6211_ST10287165_POE_P1.Models
{
    public class CartItem : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string ProductId { get; set; }
        public string ProductID { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public string ProductName { get; set; }
        public string ProductImageUrl { get; set; }     

        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public CartItem()
        {
            this.RowKey = Guid.NewGuid().ToString();
        }
    }
}
