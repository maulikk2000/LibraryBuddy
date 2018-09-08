using IdentityServer4.AspNetIdentity;
using IdentityServer4.Services;
using LibraryBuddy.Services.Identity.API.Data;
using LibraryBuddy.Services.Identity.API.Helper;
using LibraryBuddy.Services.Identity.API.Infrastructure.Filters;
using LibraryBuddy.Services.Identity.API.Models;
using LibraryBuddy.Services.Identity.API.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.HealthChecks;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace LibraryBuddy.Services.Identity.API.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
               
        public static void ConfigureApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            ConfigureServices(services, configuration);
        }

        static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            var typeFinder = new AppDomainTypeFinder();
            var startupConfigurations = typeFinder.FindClassesOfType<ILibraryBuddyStartup>();

            //create instances of startup configurations
            var instances = startupConfigurations
                .Select(s => (ILibraryBuddyStartup)Activator.CreateInstance(s))
                .OrderBy(s => s.Order);

            //configure services
            foreach (var instance in instances)
                instance.ConfigureServices(services, configuration);

            //register Auto Mapper
            AddAutoMapper(services);

            //register dependencies 
            RegisterDependencies(services);

        }

        private static void RegisterDependencies(IServiceCollection services)
        {
            services.AddTransient<IProfileService, ProfileService>();
            services.AddTransient<IPasswordHasher<ApplicationUser>, SCryptPasswordHasher<ApplicationUser>>();
            services.Configure<ScryptPasswordHasherOptions>(options =>
            {
                options.IterationCount = 16384;
                options.BlockSize = 8;
                options.ThreadCount = 1;
            });
            services.AddSingleton<IPwnedPasswordService, PwnedPasswordService>();
        }

        private static void AddAutoMapper(IServiceCollection services)
        {

        }

        public static void AddLibSqlContext(this IServiceCollection services, IConfiguration configuration)
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

            //TODO: READ CONNSTRING FROM ANOTHER HELPER CLASS
            var connString = configuration["appSettings:connectionStrings:IdentityDBConnString"];

            var migrationAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            services.AddDbContext<ApplicationDbContext>(options =>
               // options.UseLazyLoadingProxies() //here we dont need lazy loading.
                options.UseSqlServer(connString,
                    sqlServerOptionsAction: sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(migrationAssembly);
                        //Connection resiliency. 
                        sqlOptions.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                        //the below is used just for the sake of using. We may need it in the future.
                        //Use a ROW_NUMBER() in queries instead of OFFSET/FETCH. This method is backwards-compatible to SQL Server 2005.
                        sqlOptions.UseRowNumberForPaging();
                    }));
        }

        public static void AddLibAuthentication(this IServiceCollection services, IConfiguration configuration)
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

            //TODO: READ CONNSTRING FROM ANOTHER HELPER CLASS
            var connString = configuration["appSettings:connectionStrings:IdentityDBConnString"];

            var migrationAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            var builder = services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            })
            //Configuration Store support for Clients, Resources, and CORS settings
            .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = sql => sql.UseSqlServer(connString,
                                sqlServerOptionsAction: sqlOptions =>
                                {
                                    sqlOptions.MigrationsAssembly(migrationAssembly);
                                    sqlOptions.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                                    sqlOptions.UseRowNumberForPaging();
                                });
                })
            //Operational Store supports authorization grants, consents, and tokens (refresh and reference)
            .AddOperationalStore(options =>
            {
                options.ConfigureDbContext = operation => operation.UseSqlServer(connString,
                    sqlServerOptionsAction: sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(migrationAssembly);
                        sqlOptions.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                        sqlOptions.UseRowNumberForPaging();
                    });

                //TODO: LETS ADD THE BELOW FOR NOW AND RE-VISIT IT IF ANY ISSUES
                options.EnableTokenCleanup = true; //Indicates whether stale entries will be automatically cleaned up from the database. Default token clean up interval is 3600 seconds (1 hour)

            })           
            .AddAspNetIdentity<ApplicationUser>();

            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = "708996912208-9m4dkjb5hscn7cjrn5u0r4tbgkbj1fko.apps.googleusercontent.com";
                    options.ClientSecret = "wdfPY6t8H8cecgjlxud__4Gh";
                });

            //build service provider (IServiceProvider) from services (IServiceCollection)
            var serviceProvider = services.BuildServiceProvider();
            var env = serviceProvider.GetService<IHostingEnvironment>();

            if (env.IsDevelopment())
            {
                builder.AddDeveloperSigningCredential();
            }
            else
            {
                throw new Exception("need to configure key material");
            }

            services.AddCors(options =>
            {
                options.AddPolicy("IdentityCorsPolicy",
                    policy =>
                    policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            });

        }

        public static void AddLibDataProtection(this IServiceCollection services)
        {
            //Use data protection to share the cookies. To provide SSO experience apps must share the cookie.
            //https://docs.microsoft.com/en-au/aspnet/core/security/cookie-sharing?view=aspnetcore-2.1&tabs=aspnetcore2x#sharing-authentication-cookies-between-applications
            //trust decisions are shared between services with security tokens or cookies. 

            //out of available options, lets try Redis 
            //this is mostly used for Load Balanced environments, but we just use it 
            //we can have a config entry to check whether we have load balanced env or not and if so, then only we can load this settings

            var serviceProvider = services.BuildServiceProvider();
            var Configuration = serviceProvider.GetService<IConfiguration>();
            var redisConnString = Configuration["RedisDPConnString"];
            services.AddDataProtection(options =>
            {
                options.ApplicationDiscriminator = "librarybuddy";
            })
            .PersistKeysToRedis(ConnectionMultiplexer.Connect(redisConnString), "librarybuddykeys");
        }

        public static void AddPwnedPasswordHttpClient(this IServiceCollection services)
        {
            services.AddPwnedPasswordHttpClient(PwnedPasswordService.DefaultName, client =>
            {
                client.BaseAddress = new Uri("https://api.pwnedpasswords.com");
                client.DefaultRequestHeaders.Add("User-Agent", nameof(PwnedPasswordService));
            });
        }
        public static IHttpClientBuilder AddPwnedPasswordHttpClient(this IServiceCollection services, string name, Action<HttpClient> configureClient)
        {
            return services.AddHttpClient<IPwnedPasswordService, PwnedPasswordService>(name, configureClient);
            
        }

        public static void AddLibHealthCheck(this IServiceCollection services)
        {
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
                var serviceProvider = services.BuildServiceProvider();
                var Configuration = serviceProvider.GetService<IConfiguration>();
                var connString = Configuration["appSettings:connectionStrings:IdentityDBConnString"];

                var minutes = 5;
                if (int.TryParse(Configuration["HealthCheck:Timeout"], out var parsedMinutes))
                {
                    minutes = parsedMinutes;
                }

                checks.AddSqlCheck("IdentityDBCheck", connString, TimeSpan.FromMinutes(minutes));
            });
        }

        public static IMvcBuilder AddLibMvc(this IServiceCollection services)
        {
            var mvcBuilder = services
                .AddMvc(options =>
                {
                    options.Filters.Add(typeof(HttpGlobalExceptionFilter));
                    options.Filters.Add(typeof(ValidateModelStateFilter));
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            //TODO: COME BACK HERE AND ADD FLUENTVALIDATION
            //mvcBuilder.AddFluentValidation();

            return mvcBuilder;
        }
    }
}
