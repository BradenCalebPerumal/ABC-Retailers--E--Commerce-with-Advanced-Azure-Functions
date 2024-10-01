using Azure;
using Azure.Data.Tables;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CLDV6211_ST10287165_POE_P1.Models
{
    public class Client : ITableEntity
    {
        // ITableEntity properties for Azure Table Storage
        public string PartitionKey { get; set; }  // Can be a static value like "Client"
        public string RowKey { get; set; }        // Acts as the unique identifier (e.g., ClientId)

        // Timestamp and ETag for Azure Table Storage
        public DateTimeOffset? Timestamp { get; set; }
       
        public ETag ETag { get; set; }

        // Client-specific properties
        [Required]
        public string Username { get; set; }
      
        [Required]
        public string Password { get; set; }

        public string? ClientFirstName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string IdentityNum { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string CellNum { get; set; }

        // Instead of a navigation property, you would manage the relationship through identifiers or a query mechanism
        // public ICollection<Product> Products { get; set; }
    }
}
