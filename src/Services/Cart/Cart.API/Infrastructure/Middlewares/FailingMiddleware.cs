using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace LibraryBuddy.Services.Cart.API.Infrastructure.Middlewares
{
    public class FailingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly FailingOptions _options;
        private bool _mustFail;

        public FailingMiddleware(RequestDelegate next, FailingOptions options)
        {
            _next = next;
            _options = options;
            _mustFail = false;
        }

        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path;

            if (path.Equals(_options.ConfigPath, StringComparison.OrdinalIgnoreCase))
            {
                await ProcessConfigRequest(context);
                return;
            }

            if (MustFail(context))
            {
                //context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync("Failed due to Failing Middleware enabled.");
            }
            else
            {
                await _next.Invoke(context);
            }
        }

        private async Task ProcessConfigRequest(HttpContext context)
        {
            var enable = context.Request.Query.Keys.Any(k => k == "enable");
            var disable = context.Request.Query.Keys.Any(k => k == "disable");

            if(enable && disable)
            {
                throw new ArgumentException("Must use enable or disable querystring");
            }

            if (disable)
            {
                _mustFail = false;
                await SendOkResponse(context, "FailingMiddleware disabled. Further request will be processed.");
                return;
            }

            if (enable)
            {
                _mustFail = true;
                await SendOkResponse(context, "FailingMiddleware enabled. Further requests will return HTTP 500");
                return;
            }
            var failingMiddleware = _mustFail ? "enabled" : "disabled";
            await SendOkResponse(context, $"FailingMiddleware is {failingMiddleware}");
            return;
        }

        private async Task SendOkResponse(HttpContext context, string message)
        {
            //context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync(message);
        }

        private bool MustFail(HttpContext context)
        {
            return _mustFail &&
                (_options.EndPointPaths.Any(x => x == context.Request.Path.Value)
                || _options.EndPointPaths.Count == 0);
        }
    }
}
