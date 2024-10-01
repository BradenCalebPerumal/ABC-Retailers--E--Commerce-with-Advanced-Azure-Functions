using Azure.Storage.Queues;
using System.Text.Json;
using System.Threading.Tasks;
using CLDV6211_ST10287165_POE_P1.Models;
using System.Net.Http;
using System.Text;

namespace CLDV6211_ST10287165_POE_P1.Services
{
    public class QueueService
    {
        private readonly QueueClient _queueClient;
        private readonly HttpClient _httpClient;
        private readonly string _functionUrl = "https://st10287165bcpfucntion.azurewebsites.net/api/SendOrderToQueue";

        public QueueService(string connectionString, string queueName, HttpClient httpClient)
        {
            _queueClient = new QueueClient(connectionString, queueName);
            _queueClient.CreateIfNotExists();  // Ensure the queue exists
            _httpClient = httpClient;       
        }

        /* public async Task SendMessageAsync(OrderQueueMessage orderMessage)
         {
             var messageJson = JsonSerializer.Serialize(orderMessage);
             await _queueClient.SendMessageAsync(messageJson);
         }*/

        public async Task SendMessageAsync(OrderQueueMessage orderMessage)
        {
            var messageJson = JsonSerializer.Serialize(orderMessage);
            var content = new StringContent(messageJson, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_functionUrl, content);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to send message: {response.StatusCode}");
            }
        }
    }
}
//done
