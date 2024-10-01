using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CLDV6211_ST10287165_POE_P1.Models;

namespace CLDV6211_ST10287165_POE_P1.Data
{
    public class CLDV6211_ST10287165_POE_P1Context : DbContext
    {
        public CLDV6211_ST10287165_POE_P1Context (DbContextOptions<CLDV6211_ST10287165_POE_P1Context> options)
            : base(options)
        {
        }

        public DbSet<CLDV6211_ST10287165_POE_P1.Models.Product> Product { get; set; } = default!;
        public DbSet<CLDV6211_ST10287165_POE_P1.Models.User> User { get; set; } = default!;
        public DbSet<CLDV6211_ST10287165_POE_P1.Models.Customer> Customer { get; set; } = default!;




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           
            modelBuilder.Entity<Product>()
       .Property(p => p.Price)
       .HasColumnType("decimal(30, 2)");

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Customer>()
              .HasKey(c => new { c.PartitionKey, c.RowKey });

            // Ignore the ETag property
            modelBuilder.Entity<Customer>()
                .Ignore(c => c.ETag);

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<CLDV6211_ST10287165_POE_P1.Models.Client> Client { get; set; } = default!;
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<CheckoutViewModel> CheckoutViewModel { get; set; }
        public DbSet<Admin> Admins { get; set; } = default!;

    }


}
