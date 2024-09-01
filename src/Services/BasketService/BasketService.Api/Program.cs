using BasketService.Api.Core.Application.Repository;
using BasketService.Api.Core.Application.Services;
using BasketService.Api.Extensions;
using BasketService.Api.Extensions.Registration;
using BasketService.Api.Infrastructure.Repository;
using BasketService.Api.IntegrationEvents.EventHanders;
using BasketService.Api.IntegrationEvents.Events;
using EventBus.Base;
using EventBus.Base.Abstraction;
using EventBus.Factory;
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

builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient<IBasketRepository, RedisBasketRepository>();

builder.Services.AddTransient<IIdentityService, IdentityService>();

builder.Services.ConfigureAuth(builder.Configuration);

builder.Services.AddSingleton(sp => sp.ConfigureRedis(builder.Configuration));

builder.Services.ConfigureConsul(builder.Configuration);

builder.Services.AddSingleton<IEventBus>(sp =>
{

    EventBusConfig config = new()
    {
        ConnectionRetryCount = 5,
        EventNameSuffix = "IntegrationEvent",
        SubscriberClientAppName = "BasketService",
        EventBusType = EventBusType.RabbitMQ,
        Connection = new ConnectionFactory()
        {
            HostName = "RabbitMQ"
        }

    };
    return EventBusFactory.Create(config, sp);
});

builder.Services.AddTransient<OrderCreatedIntegrationEventHandler>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.RegisterWithConsul(app.Lifetime, builder.Configuration);
IEventBus eventBus = app.Services.GetRequiredService<IEventBus>();
eventBus.Subscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();
app.Run();


