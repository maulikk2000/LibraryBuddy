using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LibraryBuddy.Services.Basket.API.Infrastructure.Middlewares;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LibraryBuddy.Services.Basket.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .UseFailing(options =>
            {
                options.ConfigPath = "/Failing";
            })
            .UseHealthChecks("/hc")
            .UseStartup<Startup>()
            .ConfigureAppConfiguration((builderContext, config) =>
            {
                config.AddEnvironmentVariables();
            })
            .UseApplicationInsights();
            
    }
}
