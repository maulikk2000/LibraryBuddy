using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System;

namespace LibraryBuddy.Services.Basket.API.Infrastructure.Middlewares
{
    public class FailingStartupFilter : IStartupFilter
    {
        private readonly Action<FailingOptions> _options;

        public FailingStartupFilter(Action<FailingOptions> options)
        {
            _options = options;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                app.UseFailingMiddleware(_options);
                next(app);
            };
        }
    }
}