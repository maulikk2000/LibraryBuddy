using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Data.SqlClient;

namespace Microsoft.AspNetCore.Hosting
{
    public static class IWebHostExtension
    {
        public static IWebHost MigrateDbContext<TContext>(this IWebHost webHost, Action<TContext, IServiceProvider> seed) where TContext : DbContext
        {
            using(var scope = webHost.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<TContext>>();
                var context = services.GetService<TContext>();

                try
                {
                    logger.LogInformation($"Migrating database associated with the context {typeof(TContext).Name}");

                    var retry = Policy.Handle<SqlException>()
                                .WaitAndRetry(new TimeSpan[]
                                {
                                    TimeSpan.FromSeconds(5),
                                    TimeSpan.FromSeconds(10),
                                    TimeSpan.FromSeconds(15)
                                });

                    retry.Execute(() =>
                    {
                        context.Database.Migrate();

                        seed(context, services);
                    });
                }
                catch(Exception ex)
                {
                    logger.LogInformation(ex, $"An error occurred while migrating database on context {typeof(TContext).Name}");
                }

            }

            return webHost;
        }
    }
}
