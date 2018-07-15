using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryBuddy.Services.Identity.API.Infrastructure.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LibraryBuddy.Services.Identity.API.Infrastructure
{
    public class CommonStartup : ILibraryBuddyStartup
    {
        public int Order => 100;

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            
        }
        public void Configure(IApplicationBuilder application, IHostingEnvironment env,
            ILoggerFactory loggerFactory)
            => application.UseLibStaticFiles();



    }
}
