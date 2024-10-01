using Azure.Storage.Queues;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Tasks;
using CLDV6211_ST10287165_POE_P1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace CLDV6211_ST10287165_POE_P1.Functions
{
    public class OrderQueueFunction
    {
        private readonly QueueClient _queueClient;

        public OrderQueueFunction(QueueClient queueClient)
        {
            _queueClient = queueClient;
            _queueClient.CreateIfNotExists(); // Ensure the queue exists
        }

        [Function("SendOrderToQueue")]
        public async Task<IActionResult> SendOrderToQueue(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("SendOrderToQueue function triggered.");

            // Read the request body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var orderMessage = JsonSerializer.Deserialize<OrderQueueMessage>(requestBody);

            if (orderMessage == null)
            {
                return new BadRequestObjectResult("Invalid order message.");
            }

            // Convert the message to JSON and send it to the queue
            var messageJson = JsonSerializer.Serialize(orderMessage);
            await _queueClient.SendMessageAsync(messageJson);

            log.LogInformation($"Order {orderMessage.OrderId} enqueued for processing.");
            return new OkObjectResult($"Order {orderMessage.OrderId} enqueued successfully.");
        }
    }
}
