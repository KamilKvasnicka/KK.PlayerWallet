using KK.Common.DataStorage;
using KK.Common.Messaging;
using KK.Common.Repositories;
using KK.PlayerWalletApi.Services;
using KK.TransactionService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<WalletDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("WalletDb"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5, 
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        }));

builder.Services.AddHttpClient();
builder.Services.AddSingleton<IMessageBus, MessageBus>();
builder.Services.AddLogging(); 
builder.WebHost.UseUrls("http://localhost:5003");

builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ITransactionService, TransactionService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.MapGet("/", () => "API Gateway is running...");
app.MapGet("/health", () => Results.Ok("Healthy"));

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
//app.Run();
app.Run("http://0.0.0.0:5003");  //Docker