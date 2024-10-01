using Azure;
using Azure.Data.Tables;
using CLDV6211_ST10287165_POE_P1.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CLDV6211_ST10287165_POE_P1.Services
{
    public class CustomerService
    {
        private readonly TableClient _tableClient;
        private readonly string _functionsUrl;
        public CustomerService(string connectionString, string functionsUrl)
        {
            _tableClient = new TableClient(connectionString, "Customers");
            _tableClient.CreateIfNotExists();
            _functionsUrl = functionsUrl;
        }

        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            var customers = new List<Customer>();
            await foreach (var customer in _tableClient.QueryAsync<Customer>())
            {
                customers.Add(customer);
            }
            return customers;
        }
        public async Task<Customer> GetCustomerAsync(string rowKey)
        {
            try
            {
                Console.WriteLine($"Attempting to retrieve customer with RowKey: {rowKey}");

                // Query to find the customer by RowKey
                var query = _tableClient.QueryAsync<Customer>(filter: $"RowKey eq '{rowKey}'");
                await foreach (var customer in query)
                {
                    Console.WriteLine($"Customer found: {customer.CustEmail} with RowKey: {customer.RowKey}");
                    return customer; // Return the first matching customer
                }

                Console.WriteLine("No customer found with the provided RowKey.");
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine($"Error retrieving customer: {ex.Message}");
            }

            return null; // Return null if no matching customer is found
        }


        public async Task<Customer> FindCustomerByEmailAsync(string email)
        {
            Console.WriteLine($"Searching for customer with PartitionKey: {email}");

            // Query using the email as PartitionKey only
            var query = _tableClient.QueryAsync<Customer>(filter: $"PartitionKey eq '{email}'");

            await foreach (var customer in query)
            {
                Console.WriteLine($"Customer found: {customer.CustEmail}");
                return customer;  // Return the first matching customer
            }

            Console.WriteLine("No customer found with the provided email.");
            return null;  // Return null if no matching customer is found
        }



        /*     public async Task<bool> AddCustomerAsync(Customer customer)
             {
                 if (await FindCustomerByEmailAsync(customer.CustEmail) != null)
                 {
                     return false; // Customer already exists
                 }

                 customer.PartitionKey = customer.CustEmail; // Use email as PartitionKey for unique identification
                 customer.RowKey = Guid.NewGuid().ToString();  // Generate a unique RowKey (e.g., GUID)

                 await _tableClient.AddEntityAsync(customer);
                 return true;
             }
     */
        public async Task<bool> AddCustomerAsync(Customer customer)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Set up the request URL to call the Azure Function
                    var requestUrl = $"{_functionsUrl}AddCustomer";  // Your function URL
                    Console.WriteLine($"Sending request to URL: {requestUrl}");

                    // Set up the POST request with form-urlencoded body data (simpler than JSON)
                    var postData = new Dictionary<string, string>
            {
                { "CustEmail", customer.CustEmail },
                { "CustPassword", customer.CustPassword },
                { "CustPasswordHash", customer.CustPasswordHash }
            };

                    // Use FormUrlEncodedContent for simpler format
                    var content = new FormUrlEncodedContent(postData);

                    // Set the function key header
                    client.DefaultRequestHeaders.Add("x-functions-key", "8lJW5K4ctkwk7s48fXc6G43AnyOko7swaVTbc2l6O5VBAzFunYHDbA==");

                    // Send the POST request
                    HttpResponseMessage response = await client.PostAsync(requestUrl, content);

                    // Log the response
                    Console.WriteLine($"Received HTTP status: {response.StatusCode}");

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Customer added successfully.");
                        return true;
                    }
                    else
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"[ERROR] Failed with status code: {response.StatusCode}");
                        Console.WriteLine($"[ERROR] Response content: {responseContent}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Exception occurred: {ex.Message}");
                return false;
            }
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            try
            {
                Console.WriteLine($"Updating customer with RowKey: {customer.RowKey} and PartitionKey: {customer.PartitionKey}");

                // Update the entity in Azure Table Storage
                await _tableClient.UpdateEntityAsync(customer, ETag.All, TableUpdateMode.Replace);
                Console.WriteLine("Customer updated in Azure Table Storage.");
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine($"Failed to update customer: {ex.Message}");
                throw;  // Re-throw the exception to be handled by the calling method
            }
        }


        public async Task DeleteCustomerAsync(string partitionKey, string rowKey)
        {
            await _tableClient.DeleteEntityAsync(partitionKey, rowKey);
        }

        public async Task<bool> CustomerExistsAsync(string id)
        {
            var query = _tableClient.QueryAsync<Customer>(filter: $"RowKey eq '{id}'");
            await foreach (var customer in query)
            {
                return true;  // If we find at least one customer, return true
            }
            return false;  // If no customers are found, return false
        }
    }
}
