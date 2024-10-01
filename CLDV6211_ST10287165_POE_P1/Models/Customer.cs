using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations.Schema;

namespace CLDV6211_ST10287165_POE_P1.Models
{
    public class Customer : ITableEntity
    {
        [NotMapped]
        public int CustId { get; set; } 
        public string CustEmail { get; set; }
        public string CustPassword { get; set; }    
        public string CustPasswordHash { get; set; }

        public string PartitionKey { get; set; } = "Customer";  // Static partition key for all customers
        public string RowKey { get; set; }  // Unique identifier, such as email or a GUID
        public DateTimeOffset? Timestamp { get; set; }
        [NotMapped]
        public ETag ETag { get; set; }
    }
}
