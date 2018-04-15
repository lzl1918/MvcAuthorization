using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AuthorizationCore.Services.Internals.Reponses
{
    internal sealed class HttpUnauthorizedResult : IActionResult
    {
        public Task ExecuteResultAsync(ActionContext context)
        {
            context.HttpContext.Response.StatusCode = 401;
            return Task.CompletedTask;
        }
    }
}
