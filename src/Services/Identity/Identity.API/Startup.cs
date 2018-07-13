// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.DataProtection;
using System;
using System.Reflection;
using StackExchange.Redis;
using Microsoft.Extensions.HealthChecks;
using IdentityServer4.Services;
using LibraryBuddy.Services.Identity.API.Models;
using LibraryBuddy.Services.Identity.API.Services;
using LibraryBuddy.Services.Identity.API.Data;
using LibraryBuddy.Services.Identity.API.Helper;
using LibraryBuddy.Services.Identity.API.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;

namespace LibraryBuddy.Services.Identity.API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureApplicationServices(Configuration);
           
           

            //services.Configure<IISOptions>(iis =>
            //{
            //    iis.AuthenticationDisplayName = "Windows";
            //    iis.AutomaticAuthentication = false;
            //});
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.ConfigureRequestPipeline(env, loggerFactory);                      
        }
    }
}
