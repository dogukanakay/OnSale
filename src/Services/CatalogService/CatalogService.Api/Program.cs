using CatalogService.Api.Extensions;
using CatalogService.Api.Infrastructure;
using CatalogService.Api.Infrastructure.Context;
using Microsoft.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureDbContext(builder.Configuration);

builder.Services.Configure<CatalogSettings>(builder.Configuration.GetSection("CatalogSettings"));

var app = builder.Build();

// Migrate the database context

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MigrateDbContext<CatalogContext>((context, services) =>
{
    var env = services.GetRequiredService<IWebHostEnvironment>();
    var logger = services.GetRequiredService<ILogger<CatalogContextSeed>>();

    var seeder = new CatalogContextSeed();
    seeder.SeedAsync(context, env, logger).Wait();
});

app.Run();
