using Microsoft.AspNetCore.Builder;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;
using Ocelot.Values;
using Web.ApiGateway.Extensions;
using Web.ApiGateway.Infrastructure;
using Web.ApiGateway.Services;
using Web.ApiGateway.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider((context, options) =>
{
    options.ValidateOnBuild = false;
    options.ValidateScopes = false;
});

IConfiguration configuration = new ConfigurationBuilder().SetBasePath(builder.Environment.ContentRootPath).AddJsonFile("Configurations/ocelot.json").Build();

builder.Services.AddOcelot(configuration).AddConsul();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder.SetIsOriginAllowed((host) => true)
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
});

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddTransient<HttpClientDelegatingHandler>();
builder.Services.AddHttpClient("basket", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["urls:basket"]);
}).AddHttpMessageHandler<HttpClientDelegatingHandler>();

builder.Services.AddHttpClient("catalog", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["urls:catalog"]);
}).AddHttpMessageHandler<HttpClientDelegatingHandler>();

builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<IBasketService, BasketService>();
builder.Services.ConfigureAuth(builder.Configuration);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

await app.UseOcelot();
app.Run();
