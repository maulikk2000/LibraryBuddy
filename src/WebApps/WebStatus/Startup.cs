using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebStatus.Extensions;

namespace WebStatus
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            RegisterAppInsights(services);
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddHealthChecks( checks =>
            {
                var minutes = 5;
                if(int.TryParse(Configuration["HealthCheckTimeOut"], out var parsedMinutes))
                {
                    minutes = parsedMinutes;
                }
                //extension method to make sure URLs in config is not null
                checks.AddUrlCheckIfNotNull(Configuration["IdentityUrl"], TimeSpan.FromMinutes(minutes));
                checks.AddUrlCheckIfNotNull(Configuration["LibraryCatalogUrl"], TimeSpan.FromMinutes(minutes));
                checks.AddUrlCheckIfNotNull(Configuration["LibraryServicesUrl"], TimeSpan.FromMinutes(minutes));
                checks.AddUrlCheckIfNotNull(Configuration["CheckoutUrl"], TimeSpan.FromMinutes(minutes));
                checks.AddUrlCheckIfNotNull(Configuration["PaymentUrl"], TimeSpan.FromMinutes(minutes));
                checks.AddUrlCheckIfNotNull(Configuration["LateReturnFeeUrl"], TimeSpan.FromMinutes(minutes));
                checks.AddUrlCheckIfNotNull(Configuration["LocationUrl"], TimeSpan.FromMinutes(minutes));
                checks.AddUrlCheckIfNotNull(Configuration["LoanUrl"], TimeSpan.FromMinutes(minutes));
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            //Azure App Service provider stores logs in Azure blob storage.
            //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.1
            //As per above link, below provider only works if APP runs in Aure env.

            //The provider only works when the project runs in the Azure environment. 
            //It has no effect when the project is run locally—it doesn't write to local files or local development storage for blobs.
            loggerFactory.AddAzureWebAppDiagnostics();

            //https://github.com/Microsoft/ApplicationInsights-aspnetcore/wiki/Logging
            //Configure application insights instrumentation key
            //https://github.com/Microsoft/ApplicationInsights-aspnetcore/wiki/Getting-Started-for-a-ASP.NET-CORE-2.0-WebApp
            //When running the application from Visual Studio IDE, all traces logged via ILogger interface are automatically captured. 
            //If an instrumentation key is configured, then these traces are sent to the Application Insights service.
            loggerFactory.AddApplicationInsights(app.ApplicationServices, LogLevel.Error);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private void RegisterAppInsights(IServiceCollection services)
        {
            //Add App Insight
            services.AddApplicationInsightsTelemetry(Configuration);
            //if we decide to use kubernetes or SF, enable it here.
        }
    }
}
