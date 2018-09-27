using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using LibraryBuddy.Services.Basket.API.Infrastructure.Filters;
using LibraryBuddy.BuildingBlocks.AzureServiceBusEventBus;
using LibraryBuddy.BuildingBlocks.EventBus;
using LibraryBuddy.BuildingBlocks.EventBus.Abstractions;
using LibraryBuddy.Services.Basket.API.Infrastructure.Middlewares;
using LibraryBuddy.Services.Basket.API.IntegrationEvents.EventHandling;
using LibraryBuddy.Services.Basket.API.IntegrationEvents.Events;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.HealthChecks;
using Newtonsoft.Json;
using StackExchange.Redis;
using Microsoft.Extensions.Options;
using Microsoft.Azure.ServiceBus;
using Swashbuckle.AspNetCore.Swagger;
using Autofac.Extensions.DependencyInjection;
using LibraryBuddy.Services.Basket.API.Models;
using LibraryBuddy.Services.Basket.API.Services;

namespace LibraryBuddy.Services.Basket.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            RegisterAppInsights(services);
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc(options => 
            {
                options.Filters.Add(typeof(HttpGlobalExceptionFilter));
                options.Filters.Add(typeof(ValidateModelStateFilter));
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
            //By default, Swagger JSON will not be formatted. If the Swagger JSON should be indented properly, 
            //set the SerializerSettings option in your AddMvc helper:
            //https://github.com/domaindrivendev/Swashbuckle.AspNetCore
            .AddJsonOptions(options =>
            {
                options.SerializerSettings.Formatting = Formatting.Indented;
            });

            ConfigureAuthService(services);

            services.AddHealthChecks(checks =>
            {
                checks.AddValueTaskCheck("Basket Endpoint", () => new ValueTask<IHealthCheckResult>(HealthCheckResult.Healthy("Ok")),
                    TimeSpan.FromMinutes(5));
            });

            services.Configure<BasketSettings>(Configuration);

            services.AddSingleton<ConnectionMultiplexer>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<BasketSettings>>().Value;

                var configuration = ConfigurationOptions.Parse(settings.ConnectionString, true);
                configuration.ResolveDns = true;

                return ConnectionMultiplexer.Connect(configuration);
            });

            if (Configuration.GetValue<bool>("AzureServiceBusEnabled"))
            {
                services.AddSingleton<IServiceBusPersistedConnection>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<DefaultServiceBusPersistedConnection>>();

                    var serviceBusConnectionString = Configuration["AzureServiceBusConnection"];
                    var serviceBusConnection = new ServiceBusConnectionStringBuilder(serviceBusConnectionString);

                    return new DefaultServiceBusPersistedConnection(serviceBusConnection, logger);
                });
            }
            else
            {
                //use rabbitMQ or other here
            }

            RegisterEventBus(services);

            services.AddSwaggerGen(options =>
            {
                options.DescribeAllEnumsAsStrings();

                options.SwaggerDoc("v1", new Info
                {
                    Title = "Basket HTTP API",
                    Version = "v1",
                    Description = "The Basket Service HTTP API"
                });

                options.AddSecurityDefinition("oauth2", new OAuth2Scheme
                {
                    Type = "oauth2",
                    Flow = "implicit",
                    AuthorizationUrl = $"{Configuration.GetValue<string>("IdentityUrlExternal")}/connect/authorize",
                    TokenUrl = $"{Configuration.GetValue<string>("IdentityUrlExternal")}/connect/token",
                    Scopes = new Dictionary<string, string>()
                    {
                        { "basket", "Basket API" }
                    }
                });

                options.OperationFilter<AuthorizeCheckOperationFilter>();
            });

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IBasketRepository, RedisBasketRepository>();
            services.AddTransient<IIdentityService, IdentityService>();

            services.AddOptions();

            var container = new ContainerBuilder();
            container.Populate(services);

            return new AutofacServiceProvider(container.Build());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                //Use HTTP strict transport security header
                //HSTS is a web security policy mechanism which helps to protect websites against 
                //protocol downgrade and cookie hijacking. It allows web servers to declare that web browsers
                //should only interact with it using secure HTTPS and never via HTTP protocol.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            //app.UseMvc(routes =>
            //{
            //    routes.MapRoute(
            //        name: "default",
            //        template: "{controller=Home}/{action=Index}/{id?}");
            //});
            app.UseStaticFiles();
            ConfigureAuth(app);
            app.UseCors("LibraryCORS");

            app.UseMvcWithDefaultRoute();
            var pathBase = 
            app.UseSwagger()
                .UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Basket API V1");
                    options.OAuthClientId("basketswaggerui");
                    options.OAuthAppName("Basket Swagger UI");
                });
            ConfigureEventBus(app);
        }

        private void RegisterAppInsights(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry(Configuration);
        }

        private void ConfigureAuthService(IServiceCollection services)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            var identityUrl = Configuration.GetValue<string>("IdentityUrl");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = identityUrl;
                options.RequireHttpsMetadata = false;
                options.Audience = "basket";
            });
        }

        protected virtual void ConfigureAuth(IApplicationBuilder builder)
        {
            if (Configuration.GetValue<bool>("UseLoadTest"))
            {
                builder.UseMiddleware<ByPassAuthMiddleware>();
            }
            builder.UseAuthentication();
        }

        private void RegisterEventBus(IServiceCollection services)
        {
            var subscriptionClientName = Configuration.GetValue<string>("SubscriptionClientName");

            if (Configuration.GetValue<bool>("AzureServiceBusEnabled"))
            {
                services.AddSingleton<IEventBus, ServiceBusEventBus>(options =>
                {
                    var serviceBusPersisterConnection = options.GetRequiredService<IServiceBusPersistedConnection>();
                    var iLifeTimeScope = options.GetRequiredService<ILifetimeScope>();
                    var logger = options.GetRequiredService<ILogger<ServiceBusEventBus>>();
                    var eventBusSubscriptionManager = options.GetRequiredService<IEventBusSubscriptionManager>();

                    return new ServiceBusEventBus(serviceBusPersisterConnection, logger, eventBusSubscriptionManager, 
                        subscriptionClientName, iLifeTimeScope);
                });
            }
            else
            {
                //here add similar settings as above for RabbitMQ or any other messaging broker.
            }

            services.AddSingleton<IEventBusSubscriptionManager, InMemoryEventBusSubscriptionsManager>();

            services.AddTransient<OtherUserCheckedOutBookEventHandler>();
            services.AddTransient<CheckOutStartedIntegrationEventHandler>();
        }

        private void ConfigureEventBus(IApplicationBuilder app)
        {
            var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();

            eventBus.Subscribe<OtherUserCheckedOutBookEvent, OtherUserCheckedOutBookEventHandler>();
            eventBus.Subscribe<CheckOutStartedIntegrationEvent, CheckOutStartedIntegrationEventHandler>();
        }


    }

}
