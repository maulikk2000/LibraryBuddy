// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.EntityFramework.DbContexts;
using LibraryBuddy.Identity.API;
using LibraryBuddy.Services.Identity.API.Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Identity.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //var seed = args.Any(x => x == "/seed");
            //if (seed) args = args.Except(new[] { "/seed" }).ToArray();

            //var host = BuildWebHost(args);

            //if (seed)
            //{
            //    SeedData.EnsureSeedData(host.Services);
            //    return;
            //}

            //host.Run();

            BuildWebHost(args)
                //https://dismantledtech.wordpress.com/2014/06/07/using-underscore-to-denote-unused-parameters-in-c-lambdas/
                //If we never use an argument that’s passed, we can adopt a convention of replacing its parameter name with one or more underscores:
                .MigrateDbContext<PersistedGrantDbContext>((_, __) => { })
                .MigrateDbContext<ApplicationDbContext>((context, services) =>
                {
                    var env = services.GetService<IHostingEnvironment>();
                    var logger = services.GetService<ILogger<ApplicationDbContext>>();
                    var settings = services.GetService<IOptions<AppSettings>>();

                    new ApplicationDbContextSeed()
                    .SeedAsync(context, env, logger, settings)
                    .Wait();
                })
                .MigrateDbContext<ConfigurationDbContext>((context,services) =>
                {
                    var configuration = services.GetService<IConfiguration>();

                    new ConfigurationDbContextSeed()
                        .SeedAsync(context, configuration)
                        .Wait();
                })
                .Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                //here add AzureKeyVault configuration
                    .ConfigureAppConfiguration((context, config) => 
                    {
                        config.AddJsonFile("azurekeyvault.json", optional: false, reloadOnChange: true);
                        var builtConfig = config.Build();

                        var keyValutConfigBuilder = new ConfigurationBuilder();
                          
                        //we read the values from azurekeyvault.json file which is not checked in

                        keyValutConfigBuilder.AddAzureKeyVault(
                            $"https://{builtConfig["azureKeyVault:vault"]}.vault.azure.net/",
                                builtConfig["azureKeyVault:clientId"],
                                builtConfig["azureKeyVault:clientSecret"]
                            );
                        var keyValutConfig = keyValutConfigBuilder.Build();
                        config.AddConfiguration(keyValutConfig);
                    })   
                    .UseHealthChecks("/identityhc")
                    .UseStartup<Startup>()
                    .UseSerilog((context, configuration) =>
                    {
                        configuration
                            .MinimumLevel.Debug()
                            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                            .MinimumLevel.Override("System", LogEventLevel.Warning)
                            .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
                            .Enrich.FromLogContext()
                            .WriteTo.File(@"identityserver4_log.txt")
                            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Literate);
                    })
                    .Build();
        }
    }
}
