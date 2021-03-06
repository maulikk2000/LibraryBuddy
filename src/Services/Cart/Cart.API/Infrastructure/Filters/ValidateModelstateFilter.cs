﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryBuddy.Services.Cart.API.Infrastructure.Filters
{
    public class ValidateModelStateFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var validationError = context.ModelState
                    .Keys
                    .SelectMany(k => context.ModelState[k].Errors)
                    .Select(e => e.ErrorMessage)
                    .ToArray();

                var json = new JsonErrorResponse
                {
                    Messages = validationError
                };

                context.Result = new BadRequestObjectResult(json);
            }
            return;

        }
    }
}
