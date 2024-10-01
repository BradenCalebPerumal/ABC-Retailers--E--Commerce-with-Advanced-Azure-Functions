using Azure;
using Azure.Data.Tables;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using CLDV6211_ST10287165_POE_P1.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;      // Add this for HttpClient and StringContent
using System.Text;          // Add this for Encoding.UTF8
using System.Text.Json;     // Add this for JsonSerializer
namespace CLDV6211_ST10287165_POE_P1.Services
{
    public class OrderService
    {
        private readonly TableClient _orderTableClient;
        private readonly QueueClient _queueClient;
        private readonly TableClient _orderItemTableClient; // Ensure this is declared
                                                            // private readonly IHttpContextAccessor _httpContextAccessor;
        public OrderService(string connectionString, string queueName)
        {
            _orderTableClient = new TableClient(connectionString, "Orders");
            // Ensure the table exists
            _orderTableClient.CreateIfNotExists();

            _queueClient = new QueueClient(connectionString, "orderqueue");
            // Ensure the queue exists
            _queueClient.CreateIfNotExists();

            _orderItemTableClient = new TableClient(connectionString, "OrderItems");
            _orderItemTableClient.CreateIfNotExists(); // Ensure this is initialized
            //_httpContextAccessor = httpContextAccessor;
        }
        public async Task<List<Order>> GetAllOrdersAsync()
        {
            var orders = new List<Order>();
            await foreach (var order in _orderTableClient.QueryAsync<Order>())
            {
                orders.Add(order);
            }
            return orders;
        }
        public async Task<List<OrderItem>> GetOrderItemsByClientAsync(string clientId)
        {
            // Define the filter string based on the clientId (ProductId in this context)
            string filter = TableClient.CreateQueryFilter<OrderItem>(item => item.ProductId == clientId);

            // Query the table with the filter string
            var orderItems = new List<OrderItem>();
            await foreach (var item in _orderItemTableClient.QueryAsync<OrderItem>(filter))
            {
                orderItems.Add(item);
            }

            return orderItems;
        }
        public async Task<Order> GetOrderByRowKeyAsync(string rowKey)
        {
            try
            {
                // Define the filter query to find the order by RowKey
                string filter = $"RowKey eq '{rowKey}'";

                // Query the table with the specified filter
                var orderQuery = _orderTableClient.QueryAsync<Order>(filter);

                // Iterate through the results and return the first matching order
                await foreach (var order in orderQuery)
                {
                    return order; // Return the first match
                }

                // If no order is found, log and return null
                Console.WriteLine($"Order with RowKey {rowKey} not found.");
                return null;
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine($"Error retrieving order by RowKey: {ex.Message}");
                return null; // Handle or re-throw the exception as needed
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine($"NullReferenceException: {ex.Message}");
                return null; // Log and handle the exception as needed
            }
        }

        public async Task<Order> GetOrderAsync(string orderId)
        {
            try
            {
                var response = await _orderTableClient.GetEntityAsync<Order>(partitionKey: null, rowKey: orderId);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                // Order not found
                return null;
            }
        }
        public async Task CreateOrderAsync(Order order)
        {

            await _orderTableClient.AddEntityAsync(order);
            await SendOrderToQueueAsync(order);
        }

        public async Task CreateOrderItemAsync(OrderItem orderItem)
        {
            try
            {
                await _orderItemTableClient.AddEntityAsync(orderItem);
                Console.WriteLine($"OrderItem {orderItem.RowKey} created successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating order item: {ex.Message}");
                throw;  // Rethrow to handle it further up the call stack if necessary
            }
        }

        // Method to fetch the specific order confirmation message from the queue without deleting it
        // Method to fetch the specific order confirmation message from the queue
        // Method to fetch the specific order confirmation message from the queue without deleting it
        public async Task<string> GetOrderConfirmationMessageAsync(string orderRowKey)
        {
            // Retrieve a batch of messages from the queue
            QueueMessage[] messages = await _queueClient.ReceiveMessagesAsync(maxMessages: 10, visibilityTimeout: TimeSpan.FromSeconds(30));

            foreach (var message in messages)
            {
                // Check if the message body contains the specific OrderRowKey
                if (message.Body.ToString().Contains($"{{\"OrderId\":\"{orderRowKey}"))
                {
                    // Optionally log the found message
                    Console.WriteLine($"Found message for OrderRowKey: {orderRowKey}");

                    // Delete the message after processing
                    //await _queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);

                    // Return the message body containing the order details
                    return message.Body.ToString();
                }
            }

            // Log if no matching order is found
            Console.WriteLine($"OrderRowKey: {orderRowKey} not found in queue.");
            return "Order not found in queue.";
        }

        /*   private async Task SendOrderToQueueAsync(Order order)
           {
               var message = $"Order Number:{order.RowKey} created for customer {order.CustEmail} with total {order.TotalAmount:C}.";
               await _queueClient.SendMessageAsync(message);
               // Store RowKey in session
              // HttpContext.Session.SetString("RowKey", customer.RowKey);
               Console.WriteLine($"Order {order.RowKey} enqueued for processing.");
           }
           // Method to update an order in Azure Table Storage
   */
        /* private async Task SendOrderToQueueAsync(Order order)
         {
             var orderMessage = new OrderQueueMessage
             {
                 OrderId = order.RowKey,
                 CustomerId = order.PartitionKey,
                 Status = "Pending"  // Assuming all new orders are initially "Pending"
             };

             // Send the message to the Azure Function (HTTP POST)
             var httpClient = new HttpClient();
             var functionUrl = "https://st10287165bcpfucntion.azurewebsites.net/api/SendOrderToQueue";  // Replace with your Azure Function URL

             var messageJson = System.Text.Json.JsonSerializer.Serialize(orderMessage);
             var content = new StringContent(messageJson, System.Text.Encoding.UTF8, "application/json");

             var response = await httpClient.PostAsync(functionUrl, content);

             if (response.IsSuccessStatusCode)
             {
                 Console.WriteLine($"Order {order.RowKey} successfully sent to Azure Function for queue processing.");
             }
             else
             {
                 Console.WriteLine($"Failed to send order {order.RowKey} to Azure Function. Status code: {response.StatusCode}");
             }
         }*/
        /*   public async Task SendOrderToQueueAsync(Order order)
           {
               HttpClient client = new HttpClient();
               string functionUrl = "https://st10287165bcpfucntion.azurewebsites.net/api/SendToQueue";

               var orderMessage = new OrderQueueMessage
               {
                   OrderId = order.RowKey,
                   CustomerId = order.PartitionKey,
                   Status = "Pending"  // Assuming all new orders are initially "Pending"
               };
               string jsonPayload = JsonConvert.SerializeObject(orderMessage);
               HttpContent content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

               HttpResponseMessage response = await client.PostAsync(functionUrl, content);
               if (!response.IsSuccessStatusCode)
               {
                   throw new Exception("Failed to send message to queue via Azure Function.");
               }
           }
   */
        private async Task SendOrderToQueueAsync(Order order)
        {
            var orderMessage = new OrderQueueMessage
            {
                OrderId = order.RowKey,
                CustomerId = order.PartitionKey,
                Status = "Pending",

            };

            var messageJson = System.Text.Json.JsonSerializer.Serialize(orderMessage);
            await _queueClient.SendMessageAsync(messageJson);
            Console.WriteLine($"Order {order.RowKey} enqueued for processing.");
        }

        public async Task UpdateOrderAsync(Order order)
        {
            try
            {
                // Update the entity in Azure Table Storage using RowKey as the unique identifier
                await _orderTableClient.UpdateEntityAsync(order, ETag.All, TableUpdateMode.Replace);
                Console.WriteLine($"Order {order.RowKey} updated successfully.");
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine($"Failed to update order: {ex.Message}");
                throw; // Re-throw the exception to be handled by the calling method
            }
        }

        // Retrieves all orders for a specific customer using the customer's RowKey as the PartitionKey
        public async Task<List<Order>> GetOrdersByCustomerIdAsync(string customerId)
        {
            string filter = TableClient.CreateQueryFilter<Order>(order => order.PartitionKey == customerId);
            var orders = new List<Order>();
            await foreach (var order in _orderTableClient.QueryAsync<Order>(filter))
            {
                orders.Add(order);
            }
            return orders;
        }

        // Retrieves all order items for a specific order using the order's RowKey as the PartitionKey
        public async Task<List<OrderItem>> GetOrderItemsByOrderIdAsync(string orderId)
        {
            string filter = TableClient.CreateQueryFilter<OrderItem>(item => item.PartitionKey == orderId);
            var orderItems = new List<OrderItem>();
            await foreach (var item in _orderItemTableClient.QueryAsync<OrderItem>(filter))
            {
                orderItems.Add(item);
            }
            return orderItems;
        }


    }

}


//done everything works 