using LibraryBuddy.Services.Cart.API;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cart.FunctionalTests.Base
{
    public class CartTestStartup : Startup
    {
        public CartTestStartup(IConfiguration env) : base(env)
        {

        }

        protected override void ConfigureAuth(IApplicationBuilder builder)
        {
           if(Configuration["isTest"] == bool.TrueString.ToLowerInvariant())
            {
                builder.UseMiddleware<AutoAuthorizeMiddleware>();
            }
            else
            {
                base.ConfigureAuth(builder);
            }
        }
    }
}
