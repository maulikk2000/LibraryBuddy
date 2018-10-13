using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryBuddy.Services.Cart.API.Auth.Server
{
    public class IdentitySecurityScheme : SecurityScheme
    {
        public IdentitySecurityScheme()
        {
            Type = "IdentitySecurityScheme";
            Description = "";
            Extensions.Add("authorizationUrl", "http://localhost:5103/Auth/Client/popup.html");
            Extensions.Add("flow", "implicit");
            Extensions.Add("scopes", new List<string>
            {
                "basket"
            });
        }
    }
}
