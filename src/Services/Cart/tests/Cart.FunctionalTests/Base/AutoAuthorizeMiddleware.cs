using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Cart.FunctionalTests.Base
{
    public class AutoAuthorizeMiddleware
    {
        public const string IDENTITY_ID = "C6F30F40-31B8-46B4-A676-790810F68E4D";
        private readonly RequestDelegate _requestDelegate;

        public AutoAuthorizeMiddleware(RequestDelegate requestDelegate)
        {
            _requestDelegate = requestDelegate;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var identity = new ClaimsIdentity("cookies");

            identity.AddClaim(new Claim("sub", IDENTITY_ID));
            identity.AddClaim(new Claim("unique_name", IDENTITY_ID));

            httpContext.User.AddIdentity(identity);

            await _requestDelegate.Invoke(httpContext);
        }
    }
}
