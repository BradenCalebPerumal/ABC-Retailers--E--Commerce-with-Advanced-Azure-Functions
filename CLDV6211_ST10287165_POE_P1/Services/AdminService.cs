using Azure;
using Azure.Data.Tables;
using CLDV6211_ST10287165_POE_P1.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CLDV6211_ST10287165_POE_P1.Services
{
    public class AdminService
    {
        private readonly TableClient _tableClient;

        public AdminService(string connectionString)
        {
            // Initialize the TableClient for Azure Table Storage
            _tableClient = new TableClient(connectionString, "Admins");
            _tableClient.CreateIfNotExists();
        }

        // Retrieve all admins from the table
        public async Task<List<Admin>> GetAllAdminsAsync()
        {
            var admins = new List<Admin>();
            await foreach (var admin in _tableClient.QueryAsync<Admin>())
            {
                admins.Add(admin);
            }
            return admins;
        }

        // Retrieve a specific admin by RowKey
        public async Task<Admin> GetAdminAsync(string rowKey)
        {
            try
            {
                Console.WriteLine($"Attempting to retrieve admin with RowKey: {rowKey}");

                // Query to find the admin by RowKey
                var query = _tableClient.QueryAsync<Admin>(filter: $"RowKey eq '{rowKey}'");
                await foreach (var admin in query)
                {
                    Console.WriteLine($"Admin found: {admin.AdminEmail} with RowKey: {admin.RowKey}");
                    return admin; // Return the first matching admin
                }

                Console.WriteLine("No admin found with the provided RowKey.");
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine($"Error retrieving admin: {ex.Message}");
            }

            return null; // Return null if no matching admin is found
        }

        // Find an admin by their email
        public async Task<Admin> FindAdminByEmailAsync(string email)
        {
            Console.WriteLine($"Searching for admin with AdminEmail: {email}");

            // Query using the email for finding the admin
            var query = _tableClient.QueryAsync<Admin>(filter: $"PartitionKey eq 'Admin' and AdminEmail eq '{email}'");

            await foreach (var admin in query)
            {
                Console.WriteLine($"Admin found: {admin.AdminEmail}");
                return admin;  // Return the first matching admin
            }

            Console.WriteLine("No admin found with the provided email.");
            return null;  // Return null if no matching admin is found
        }

        // Add a new admin to the table
        public async Task<bool> AddAdminAsync(Admin admin)
        {
            // Check if an admin with the same email already exists
            if (await FindAdminByEmailAsync(admin.AdminEmail) != null)
            {
                return false; // Admin already exists
            }

            admin.PartitionKey = "Admin"; // Static partition key
            admin.RowKey = Guid.NewGuid().ToString();  // Generate a unique RowKey (GUID)

            try
            {
                await _tableClient.AddEntityAsync(admin);
                Console.WriteLine($"Admin added with RowKey: {admin.RowKey}");
                return true;
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine($"Failed to add admin: {ex.Message}");
                return false;
            }
        }

        // Update an existing admin's details
        public async Task UpdateAdminAsync(Admin admin)
        {
            try
            {
                Console.WriteLine($"Updating admin with RowKey: {admin.RowKey} and PartitionKey: {admin.PartitionKey}");

                // Update the entity in Azure Table Storage
                await _tableClient.UpdateEntityAsync(admin, ETag.All, TableUpdateMode.Replace);
                Console.WriteLine("Admin updated in Azure Table Storage.");
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine($"Failed to update admin: {ex.Message}");
                throw;  // Re-throw the exception to be handled by the calling method
            }
        }

        // Delete an admin by PartitionKey and RowKey
        public async Task DeleteAdminAsync(string partitionKey, string rowKey)
        {
            try
            {
                Console.WriteLine($"Deleting admin with PartitionKey: {partitionKey} and RowKey: {rowKey}");
                await _tableClient.DeleteEntityAsync(partitionKey, rowKey);
                Console.WriteLine("Admin deleted successfully.");
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine($"Failed to delete admin: {ex.Message}");
                throw;
            }
        }

        // Check if an admin exists by RowKey
        public async Task<bool> AdminExistsAsync(string id)
        {
            var query = _tableClient.QueryAsync<Admin>(filter: $"RowKey eq '{id}'");
            await foreach (var admin in query)
            {
                return true;  // If we find at least one admin, return true
            }
            return false;  // If no admins are found, return false
        }
    }
}
