﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace OcelotApiGw
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddJsonFile("ocelot.json");
            })
            //http://ocelot.readthedocs.io/en/latest/introduction/gettingstarted.html
            //from .net core 2.1, as per doco add Ocelot entries in Program.cs, 
            .ConfigureServices(s =>
            {
                //Instead of adding the configuration directly e.g.AddJsonFile(“ocelot.json”) you can call AddOcelot()
                s.AddOcelot();
            })
            .Configure(app =>
            {
                app.UseOcelot().Wait();
            })
                .UseStartup<Startup>();
    }
}
