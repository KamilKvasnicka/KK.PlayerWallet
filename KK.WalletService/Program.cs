using KK.Common.Messaging;
using KK.TransactionService.Services;
using KK.WalletService.Services;
using Microsoft.EntityFrameworkCore;
using KK.Common.Repositories;
using KK.Common.DataStorage;

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

builder.Services.AddScoped<IWalletRepository, WalletRepository>(); 
builder.Services.AddScoped<IGameWalletService, GameWalletService>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ITransactionService, TransactionService>();

builder.Services.AddSingleton<IMessageBus, MessageBus>(); 
builder.Services.AddSingleton<IRabbitMqConsumer, RabbitMqConsumer>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.WebHost.UseUrls("http://0.0.0.0:8080");
var app = builder.Build();

app.MapGet("/", () => "Wallet Service API is running...");
app.MapGet("/health", () => Results.Ok("Healthy"));
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

var rabbitMqConsumer = app.Services.GetRequiredService<IRabbitMqConsumer>();
_ = Task.Run(async () => await rabbitMqConsumer.StartAsync());

app.Run();


