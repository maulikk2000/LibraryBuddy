using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Cart.FunctionalTests.Base
{
    public class CartScenarioBase
    {
        private const string ApiUrlBase = "api/v1/cart";

        public TestServer CreateServer()
        {
            var path = Assembly.GetAssembly(typeof(CartScenarioBase)).Location;

            var hostBuilder = new WebHostBuilder()
                .UseContentRoot(Path.GetDirectoryName(path))
                .ConfigureAppConfiguration(options =>
                {
                    options.AddJsonFile("appsettings.json", optional: false)
                    .AddEnvironmentVariables();
                }).UseStartup<CartTestStartup>();

            return new TestServer(hostBuilder);
        }

        public static class GetCartUrlwithId
        {
            public static string GetCart(int id)
            {
                return $"{ ApiUrlBase}/{id}";
            }
        }

        public static class PostCartUrl
        {
            public static string Cart = $"{ApiUrlBase}/";
            public static string CheckoutOrder = $"{ApiUrlBase}/checkout";
        }
    }
}
