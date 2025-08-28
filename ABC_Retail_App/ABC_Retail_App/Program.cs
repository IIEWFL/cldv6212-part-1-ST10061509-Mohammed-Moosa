using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Queues;
using Azure.Data.Tables;
using Azure.Storage.Files.Shares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection; // Required for GetRequiredService
using System;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure Azure Storage Clients
var storageConnectionString = builder.Configuration.GetConnectionString("AzureStorage");

// Register Azure Storage Clients as Singletons
builder.Services.AddSingleton(x => new BlobServiceClient(storageConnectionString));
builder.Services.AddSingleton(x => new QueueServiceClient(storageConnectionString));
builder.Services.AddSingleton(x => new TableServiceClient(storageConnectionString));
builder.Services.AddSingleton(x => new ShareServiceClient(storageConnectionString));


var app = builder.Build();

// ⭐ NEW: Centralized Asynchronous Initialization of Azure Storage Resources ⭐
// This section ensures all necessary containers/tables/queues/shares exist once at startup.
using (var scope = app.Services.CreateScope())
{
    var blobServiceClient = scope.ServiceProvider.GetRequiredService<BlobServiceClient>();
    var queueServiceClient = scope.ServiceProvider.GetRequiredService<QueueServiceClient>();
    var tableServiceClient = scope.ServiceProvider.GetRequiredService<TableServiceClient>();
    var shareServiceClient = scope.ServiceProvider.GetRequiredService<ShareServiceClient>();

    // Blob Container Initialization
    var blobContainerClient = blobServiceClient.GetBlobContainerClient("product-images");
    await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
    Console.WriteLine("Azure Blob Container 'product-images' ensured to exist.");

    // Queue Initialization
    var orderQueueClient = queueServiceClient.GetQueueClient("order-processing");
    await orderQueueClient.CreateIfNotExistsAsync();
    Console.WriteLine("Azure Queue 'order-processing' ensured to exist.");

    var inventoryQueueClient = queueServiceClient.GetQueueClient("inventory-management");
    await inventoryQueueClient.CreateIfNotExistsAsync();
    Console.WriteLine("Azure Queue 'inventory-management' ensured to exist.");

    // Table Initialization
    var customerTableClient = tableServiceClient.GetTableClient("CustomerProfiles");
    await customerTableClient.CreateIfNotExistsAsync(); // Use async version
    Console.WriteLine("Azure Table 'CustomerProfiles' ensured to exist.");

    // File Share Initialization
    var contractsShareClient = shareServiceClient.GetShareClient("contracts");
    await contractsShareClient.CreateIfNotExistsAsync();
    Console.WriteLine("Azure File Share 'contracts' ensured to exist.");
}


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
