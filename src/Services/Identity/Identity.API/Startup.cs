// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Identity.API.Data;
using Identity.API.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.DataProtection;
using System;
using System.Data.SqlClient;
using System.Reflection;
using StackExchange.Redis;
using Microsoft.Extensions.HealthChecks;

namespace Identity.API
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
            //here instead of reading conn string (mostly due to pwd) from appsettings.json 
            //read the value from secrets.json file ( https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-2.1&tabs=windows)

            //var connString = Configuration.GetConnectionString("IdentityDBConnString");
            //var sqlBuilder = new SqlConnectionStringBuilder(Configuration.GetConnectionString("IdentityDBConnString"));
            //sqlBuilder.Password = Configuration["IdentityDbPwd"];
            //var connString = sqlBuilder.ConnectionString;

            //below is the way to get the value from Azure key vault
            //Azure key vault is setup in Program.cs file

            //https://www.youtube.com/watch?v=cdoY_pnqPiA
            //https://docs.microsoft.com/en-us/aspnet/core/security/key-vault-configuration?view=aspnetcore-2.1&tabs=aspnetcore2x
            var connString = Configuration["appSettings:connectionStrings:IdentityDBConnString"];

            var migrationAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connString, 
                    sqlServerOptionsAction: sqlOptions => 
                    {
                        sqlOptions.MigrationsAssembly(migrationAssembly);
                        //Connection resiliency. 
                        sqlOptions.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                    }));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc();

            services.Configure<IISOptions>(iis =>
            {
                iis.AuthenticationDisplayName = "Windows";
                iis.AutomaticAuthentication = false;
            });

            var builder = services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            })
            .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = sql => sql.UseSqlServer(connString,
                                sqlServerOptionsAction: sqlOptions =>
                                {
                                    sqlOptions.MigrationsAssembly(migrationAssembly);
                                    sqlOptions.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                                });
                })
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = operation => operation.UseSqlServer(connString,
                        sqlServerOptionsAction: sqlOptions =>
                        {
                            sqlOptions.MigrationsAssembly(migrationAssembly);
                            sqlOptions.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                        });
                })
                //.AddInMemoryIdentityResources(Config.GetIdentityResources())
                //.AddInMemoryApiResources(Config.GetApis())
                //.AddInMemoryClients(Config.GetClients())
                .AddAspNetIdentity<ApplicationUser>();

            //Use data protection to share the cookies. To provide SSO experience apps must share the cookie.
            //https://docs.microsoft.com/en-au/aspnet/core/security/cookie-sharing?view=aspnetcore-2.1&tabs=aspnetcore2x#sharing-authentication-cookies-between-applications
            //trust decisions are shared between services with security tokens or cookies. 

            //out of available options, lets try Redis 
            //this is mostly used for Load Balanced environments, but we just use it 
            //we can have a config entry to check whether we have load balanced env or not and if so, then only we can load this settings
            var redisConnString = Configuration["RedisDPConnString"];
            services.AddDataProtection(options =>
            {
                options.ApplicationDiscriminator = "librarybuddy";
            })
            .PersistKeysToRedis(ConnectionMultiplexer.Connect(redisConnString), "librarybuddykeys");

            //Add Health Check
            // https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/implement-resilient-applications/monitor-app-health
            //The process works like this: each microservice exposes the endpoint /hc. 
            //That endpoint is created by the HealthChecks library ASP.NET Core middleware. 
            //When that endpoint is invoked, it runs all the health checks that are configured in the 
            //AddHealthChecks method in the Startup class.
            services.AddHealthChecks(checks =>
            {
                //By default, the cache duration is internally set to 5 minutes
                //Since you do not want to cause a Denial of Service (DoS) in your services, 
                //or you simply do not want to impact service performance by checking resources too 
                //frequently, you can cache the returns and configure a cache duration for each health check.

                var minutes = 5;
                if (int.TryParse(Configuration["HealthCheck:Timeout"], out var parsedMinutes))
                {
                    minutes = parsedMinutes;
                }

                checks.AddSqlCheck("IdentityDBCheck", connString, TimeSpan.FromMinutes(minutes));
            });

            if (Environment.IsDevelopment())
            {
                builder.AddDeveloperSigningCredential();
            }
            else
            {
                throw new Exception("need to configure key material");
            }

            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = "708996912208-9m4dkjb5hscn7cjrn5u0r4tbgkbj1fko.apps.googleusercontent.com";
                    options.ClientSecret = "wdfPY6t8H8cecgjlxud__4Gh";
                });
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseIdentityServer();
            app.UseMvcWithDefaultRoute();
        }
    }
}
