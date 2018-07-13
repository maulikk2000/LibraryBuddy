
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace LibraryBuddy.Services.Identity.API.Infrastructure.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        
        public static void ConfigureRequestPipeline(this IApplicationBuilder application, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            ConfigurePipeline(application, env, loggerFactory);
        }

        public static void ConfigurePipeline(IApplicationBuilder application, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var typeFinder = new AppDomainTypeFinder();
            var startupConfigurations = typeFinder.FindClassesOfType<ILibraryBuddyStartup>();

            //create instances of startup configurations
            var instances = startupConfigurations
                .Select(s => (ILibraryBuddyStartup)Activator.CreateInstance(s))
                .OrderBy(s => s.Order);

            //configure services
            foreach (var instance in instances)
                instance.Configure(application, env, loggerFactory);
        }

        public static void UseLibExceptionHandler(this IApplicationBuilder application, IHostingEnvironment env)
        {          
            if (env.IsDevelopment())
            {
                application.UseDeveloperExceptionPage();
                application.UseDatabaseErrorPage();
            }
            else
            {
                application.UseExceptionHandler("/Home/Error");
            }
        }

        public static void UseBadRequestResult(this IApplicationBuilder application, ILoggerFactory loggerFactory)
        {

            application.UseStatusCodePages(context =>
            {
                if (context.HttpContext.Response.StatusCode == StatusCodes.Status400BadRequest)
                {
                    ILogger logger = loggerFactory.CreateLogger("Status404");
                    logger.LogError("Error 404.Bad Request");
                }
                return Task.CompletedTask;
            });
        }

        public static void UsePageNotFound(this IApplicationBuilder application)
        {
            application.UseStatusCodePages(async context =>
            {
                if (context.HttpContext.Response.StatusCode == StatusCodes.Status404NotFound)
                {
                    var originalPath = context.HttpContext.Request.Path;
                    var originalQueryString = context.HttpContext.Request.QueryString;

                    context.HttpContext.Features.Set<IStatusCodeReExecuteFeature>(new StatusCodeReExecuteFeature()
                    {
                        OriginalPathBase = context.HttpContext.Request.PathBase.Value,
                        OriginalPath = originalPath.Value,
                       OriginalQueryString = originalQueryString.HasValue ? originalQueryString.Value : null 
                    });

                    context.HttpContext.Request.Path = "/page-not-found";
                    context.HttpContext.Request.QueryString = QueryString.Empty;

                    try
                    {
                        await context.Next(context.HttpContext);
                    }
                    finally
                    {
                        context.HttpContext.Request.QueryString = originalQueryString;
                        context.HttpContext.Request.Path = originalPath;
                        context.HttpContext.Features.Set<IStatusCodeReExecuteFeature>(null);
                    }
                }
            });
        }

        public static void UseLibAuthenticaton(this IApplicationBuilder application)
        {
            application.UseIdentityServer();
        }

        public static void UseLibMvc(this IApplicationBuilder application)
        {
            application.UseMvcWithDefaultRoute();
        }

        public static void UseLibStaticFiles(this IApplicationBuilder application)
        {
            application.UseStaticFiles();
        }
    }
}
