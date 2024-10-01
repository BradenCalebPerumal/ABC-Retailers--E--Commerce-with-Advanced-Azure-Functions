using Azure.Data.Tables;
using Azure;

namespace CLDV6211_ST10287165_POE_P1.Models
{

    public class Order : ITableEntity
    {
        public string PartitionKey { get; set; } // CustomerId (RowKey of Customer)
        public string RowKey { get; set; } // Unique OrderId
        public string OrderStatus { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string CustEmail { get; set; }
        public string ShippingAddress { get; set; }
        public string PaymentMethod { get; set; } 
        public double TotalAmount { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public IEnumerable<OrderItem> OrderItems { get; set; }

        public Order()
        {
            this.RowKey = Guid.NewGuid().ToString(); // Generates a unique identifier for each order
        }
    }
}