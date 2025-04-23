using System;
using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using MongoDB.Driver;
using CurrencyData;
using Microsoft.Extensions.Options;
using CurrencyService; 

var builder = WebApplication.CreateBuilder(args);

// 1. Add CoreWCF support
builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();

// 2. Bind MongoDbSettings from appsettings.json
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDb"));
var mongoSettings = builder.Configuration
    .GetSection("MongoDb")
    .Get<MongoDbSettings>()!;

// Add HttpClient factory for NBP API calls
builder.Services.AddHttpClient();

// 3. Register MongoClient and IMongoDatabase
builder.Services.AddSingleton<IMongoClient>(_ =>
    new MongoClient(mongoSettings.ConnectionString));
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IMongoClient>()
      .GetDatabase(mongoSettings.DatabaseName));

// 4. Register your repositories
// We'll implement repositories when needed
builder.Services.AddScoped<CurrencyData.Repositories.UserRepository>();
// builder.Services.AddScoped<BalanceRepository>();
// builder.Services.AddScoped<TransactionRepository>();

// 5. Register your WCF service implementation 
builder.Services.AddScoped<Service>();

var app = builder.Build();

// 6. Configure WCF endpoints
app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<Service>();

    var binding = new BasicHttpBinding();
    binding.Security.Mode = BasicHttpSecurityMode.None;
    
    // Make the binding more explicitly compatible with System.ServiceModel client
    binding.MaxReceivedMessageSize = 10 * 1024 * 1024; // 10MB
    
    serviceBuilder.AddServiceEndpoint<Service, IService>(
        binding,
        "/"
    );

    // Enable metadata (WSDL) at ?wsdl
    var smb = app.Services.GetRequiredService<ServiceMetadataBehavior>();
    smb.HttpGetEnabled = true;
});

app.Run();