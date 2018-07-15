using LibraryBuddy.Services.Identity.API.Infrastructure;
using LibraryBuddy.Services.Identity.API.Infrastructure.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryBuddy.Services.Identity.Infrastructure
{
    public class HealthCheckStartup : ILibraryBuddyStartup
    {
        public int Order => 1050;

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
            => services.AddLibHealthCheck();

        public void Configure(IApplicationBuilder application, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            
        }

        
    }
}
