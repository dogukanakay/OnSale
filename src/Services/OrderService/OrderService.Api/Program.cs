using EventBus.Base;
using EventBus.Base.Abstraction;
using EventBus.Factory;
using OrderService.Api.Extensions;
using OrderService.Api.Extensions.Registration;
using OrderService.Api.Extensions.Registration.Consul;
using OrderService.Api.IntegrationEvents.EventHandlers;
using OrderService.Api.IntegrationEvents.Events;
using OrderService.Application;
using OrderService.Infrastructure.Context;
using OrderService.Persistence;
using OrderService.Persistence.Context;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseDefaultServiceProvider((context, options) =>
{
    options.ValidateScopes = false;
    options.ValidateOnBuild = false;
});
// Add services to the container.
builder.Configuration.AddEnvironmentVariables();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging(configure => configure.AddConsole()).
    AddApplicationRegistration().
    AddPersistenceRegistration(builder.Configuration).
    ConfigureEventHandlers().
    AddServiceDiscoveryRegistration(builder.Configuration);

builder.Services.AddSingleton(sp =>
{
    EventBusConfig config = new()
    {
        ConnectionRetryCount = 5,
        EventNameSuffix = "IntegrationEvent",
        SubscriberClientAppName = "OrderService",
        EventBusType = EventBusType.RabbitMQ,
        Connection = new ConnectionFactory()
        {
            HostName = "RabbitMQ"
        }

    };
    return EventBusFactory.Create(config, sp);
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MigrateDbContext<OrderDbContext>((context, service) =>
{
    var logger = service.GetService<ILogger<OrderDbContext>>();
    var dbContextSeeder = new OrderDbContextSeed();
    dbContextSeeder.SeedAsync(context, logger).Wait();
});
IEventBus eventBus = app.Services.GetRequiredService<IEventBus>();
eventBus.Subscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();

app.RegisterWithConsul(app.Lifetime, builder.Configuration);
app.Run();
