using KK.Common.Messaging;
using KK.TransactionService.Services;
using KK.Common.Repositories;
using KK.Common.DataStorage;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<WalletDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("WalletDb"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5, // Počet opakovaných pokusov
                maxRetryDelay: TimeSpan.FromSeconds(10), // Maximálna doba čakania medzi pokusmi
                errorNumbersToAdd: null);
        }));

builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddSingleton<IMessageBus, MessageBus>();
builder.Services.AddSingleton<IRabbitMqConsumer, RabbitMqConsumer>(); 
builder.Services.AddScoped<IWalletRepository, WalletRepository>(); 

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

var rabbitMqConsumer = app.Services.GetRequiredService<IRabbitMqConsumer>();
_ = Task.Run(async () => await rabbitMqConsumer.StartAsync());

app.MapGet("/health", () => "Healthy");

app.Run();
