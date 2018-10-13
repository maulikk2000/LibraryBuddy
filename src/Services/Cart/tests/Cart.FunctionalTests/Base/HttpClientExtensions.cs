using Microsoft.AspNetCore.TestHost;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Cart.FunctionalTests.Base
{
    public static class HttpClientExtensions
    {
        public static HttpClient CreateIdempotentClient(this TestServer testServer)
        {
            var client = testServer.CreateClient();

            client.DefaultRequestHeaders.Add("x-requestid", Guid.NewGuid().ToString());
            return client;
        }
    }
}
