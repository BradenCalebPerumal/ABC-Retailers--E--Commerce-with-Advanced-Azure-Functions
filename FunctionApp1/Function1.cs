using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Newtonsoft.Json;
using Azure.Data.Tables;
using CLDV6211_ST10287165_POE_P1.Models;
using Azure;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using iTextSharp.text.log;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;


namespace FunctionApp1
{
    public class Function1
    {
        [Function("SignUp")]
        public static async Task<IActionResult> Runn(
          [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
          ILogger log,
          ExecutionContext context)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string email = data?.email;
            string password = data?.password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return new BadRequestObjectResult("Email and Password are required.");
            }

            var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage"); // Ensure this is set in your application settings
            var tableClient = new TableServiceClient(connectionString).GetTableClient("Customers");
            await tableClient.CreateIfNotExistsAsync();

            var entity = new TableEntity
            {
                ["PartitionKey"] = email,  // This sets the partition key
                ["RowKey"] = Guid.NewGuid().ToString(),  // This creates a unique row key
                ["CustEmail"] = email,  // Use the correct column name as per your table design
                ["CustPassword"] = password,

                ["CustPasswordHash"] = HashPassword(password),
                ["CustId"] = 0
            };

            try
            {
                await tableClient.AddEntityAsync(entity);
                return new OkObjectResult("Customer registered successfully.");
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult("Failed to add customer. Email may already exist.");
            }
        }

        private static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", string.Empty);
            }
        }
    }
}