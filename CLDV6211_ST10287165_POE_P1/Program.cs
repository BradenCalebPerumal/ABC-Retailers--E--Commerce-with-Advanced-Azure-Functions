using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using CLDV6211_ST10287165_POE_P1.Data;
using CLDV6211_ST10287165_POE_P1.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<CLDV6211_ST10287165_POE_P1Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CLDV6211_ST10287165_POE_P1Context") ?? throw new InvalidOperationException("Connection string 'CLDV6211_ST10287165_POE_P1Context' not found.")));
// Add DbContext configuration
builder.Services.AddDbContext<CLDV6211_ST10287165_POE_P1Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CLDV6211_ST10287165_POE_P1Context") ?? throw new InvalidOperationException("Connection string 'CLDV6211_ST10287165_POE_P1Context' not found.")));
// Register the services using the single connection string
builder.Services.AddScoped<ClientService>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("AzureStorage");
    return new ClientService(connectionString);
});
builder.Services.AddSingleton<FileShareService>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("AzureStorage");

    return new FileShareService(connectionString); // No need to pass shareName anymore
});


builder.Services.AddScoped<CustomerService>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("AzureStorage");
    var functionsUrl = configuration.GetValue<string>("FunctionUrl"); // Assuming 'FunctionUrl' is the key in your configuration
    return new CustomerService(connectionString, functionsUrl);
});


// Add services to the container.
builder.Services.AddScoped<OrderService>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("AzureStorage");
    var queueName = configuration["orderqueue"]; // Ensure this is in your configuration
    return new OrderService(connectionString, queueName);
});


builder.Services.AddScoped<AdminService>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("AzureStorage"); // Ensure your Azure Storage connection string is configured
    return new AdminService(connectionString);
});
// Register CartService
builder.Services.AddScoped<CartService>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("AzureStorage");
    return new CartService(connectionString);
});
builder.Services.AddScoped<ProductService>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("AzureStorage");

    var blobServiceClient = new BlobServiceClient(connectionString);

    return new ProductService(connectionString, connectionString, blobServiceClient);
});

builder.Services.AddSingleton<IBlobStorageService, BlobStorageService>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("AzureStorage");
    return new BlobStorageService(connectionString);
});

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();  // This registers IHttpClientFactory which can be used to create HttpClient instances

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();