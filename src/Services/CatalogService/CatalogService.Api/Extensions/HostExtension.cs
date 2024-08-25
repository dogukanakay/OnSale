using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Data.SqlClient;
using Polly;
using Microsoft.Data.SqlClient;

public static class HostExtension
{
    public static WebApplication MigrateDbContext<TContext>(this WebApplication app, Action<TContext, IServiceProvider> seeder)
        where TContext : DbContext
    {
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<TContext>>();
            var context = services.GetService<TContext>();

            if (context == null)
            {
                logger.LogError("Failed to get a database context of type {DbContextName}", typeof(TContext).Name);
                return app;
            }

            try
            {
                logger.LogInformation("Migrating database associated with context {DbContextName}", typeof(TContext).Name);

                var retry = Policy.Handle<SqlException>()
                         .WaitAndRetry(new TimeSpan[]
                         {
                             TimeSpan.FromSeconds(3),
                             TimeSpan.FromSeconds(5),
                             TimeSpan.FromSeconds(8),
                         });

                retry.Execute(() => InvokeSeeder(seeder, context, services));

                logger.LogInformation("Migrated database associated with context {DbContextName}", typeof(TContext).Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while migrating the database used on context {DbContextName}", typeof(TContext).Name);
            }
        }

        return app;
    }

    private static void InvokeSeeder<TContext>(Action<TContext, IServiceProvider> seeder, TContext context, IServiceProvider services)
        where TContext : DbContext
    {
        context.Database.EnsureCreated();
        context.Database.Migrate();
        seeder(context, services);
    }
}
