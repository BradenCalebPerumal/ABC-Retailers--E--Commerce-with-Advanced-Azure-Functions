using Azure;
using Azure.Data.Tables;
using CLDV6211_ST10287165_POE_P1.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CLDV6211_ST10287165_POE_P1.Services
{
    public class ClientService
    {
        private readonly TableClient _tableClient;

        public ClientService(string tableStorageConnectionString)
        {
            // Initialize the TableClient for the "Clients" table
            _tableClient = new TableClient(tableStorageConnectionString, "Clients");
            _tableClient.CreateIfNotExists();
        }

        // Create a new client
        public async Task<Client> AddClientAsync(Client client)
        {
            // Set PartitionKey and RowKey for the client
            client.PartitionKey = "Client";  // Static PartitionKey, or adjust based on logic
            client.RowKey = Guid.NewGuid().ToString();  // Use GUID as unique identifier

            await _tableClient.AddEntityAsync(client);
            return client;
        }


        // Retrieve a client by its RowKey
        public async Task<Client> GetClientByIdAsync(string rowKey)
        {
            try
            {
                var client = await _tableClient.GetEntityAsync<Client>("Client", rowKey);
                return client;
            }
            catch (RequestFailedException)
            {
                return null; // Return null if the client is not found
            }
        }

        // Retrieve all clients
        public async Task<List<Client>> GetAllClientsAsync()
        {
            var clients = new List<Client>();
            await foreach (var client in _tableClient.QueryAsync<Client>())
            {
                clients.Add(client);
            }
            return clients;
        }

        // Update an existing client
        public async Task UpdateClientAsync(Client client)
        {
            try
            {
                await _tableClient.UpdateEntityAsync(client, ETag.All, TableUpdateMode.Replace);
            }
            catch (RequestFailedException ex)
            {
                throw new Exception("Failed to update client", ex);
            }
        }

        // Delete a client by its RowKey
        public async Task DeleteClientAsync(string rowKey)
        {
            await _tableClient.DeleteEntityAsync("Client", rowKey);
        }

        // Find a client by its username
        public async Task<Client> FindClientByUsernameAsync(string email)
        {
            var query = _tableClient.QueryAsync<Client>(filter: $"Email eq '{email}'");
            await foreach (var client in query)
            {
                return client; // Return the first match
            }
            return null; // No match found
        }
    }
}
