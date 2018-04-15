using AuthorizationCore.Helpers;
using AuthorizationCore.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace AuthorizationCore.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class AuthorizationFailedHandlerAttribute : Attribute
    {
        internal object[] ConstructParameters { get; }
        internal Type Handler { get; }

        public AuthorizationFailedHandlerAttribute(Type handler, params object[] constructParameters)
        {
            Handler = handler;
            ConstructParameters = constructParameters;
        }

        internal IActionResult Execute(HttpContext httpContext, IAuthorizationResult result)
        {
            return FailedHelper.ExecuteHandler(Handler, ConstructParameters, httpContext, result);
        }
    }
}
