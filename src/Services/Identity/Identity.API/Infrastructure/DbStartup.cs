using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LibraryBuddy.Services.Identity.API.Data;
using LibraryBuddy.Services.Identity.API.Infrastructure.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LibraryBuddy.Services.Identity.API.Infrastructure
{
    public class DbStartup : ILibraryBuddyStartup
    {
        public int Order => 10;

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration) 
            => services.AddLibSqlContext(configuration);

        public void Configure(IApplicationBuilder application, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            
        }

        
    }
}
