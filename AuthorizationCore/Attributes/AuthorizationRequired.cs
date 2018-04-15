using AuthorizationCore.Helpers;
using AuthorizationCore.Services;
using AuthorizationCore.Services.Internals;
using AuthorizationCore.Services.Internals.Reponses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AuthorizationCore.Attributes
{
    public enum AuthorizationFailedAction
    {
        KeepUnauthorized,
        Return401,
        CustomHandler
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class AuthorizationRequiredAttribute : Attribute, IAuthorizationFilter
    {
        internal static Type AuthorizationServiceType { get; set; }
        internal static MethodInfo AuthorizationServiceTryAuthorizeMethod { get; set; }
        private readonly string policyExpression;
        private readonly AuthorizationFailedAction failedAction;
        private readonly bool failedIfNotHandled;

        public AuthorizationRequiredAttribute(string expression, AuthorizationFailedAction failedAction = AuthorizationFailedAction.Return401, bool failedIfNotHandled = true)
        {
            if (string.IsNullOrWhiteSpace(expression))
                throw new Exception();
            this.policyExpression = expression;
            this.failedAction = failedAction;
            this.failedIfNotHandled = failedIfNotHandled;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            IServiceProvider services = context.HttpContext.RequestServices;
            object authorization = services.GetRequiredService(AuthorizationServiceType);
            IAuthorizationResultAccessor accessor = services.GetRequiredService<IAuthorizationResultAccessor>();
            PolicyResult policyResult = (PolicyResult)AuthorizationServiceTryAuthorizeMethod.Invoke(authorization, new object[] { policyExpression });
            SortedSet<string> succeeded = new SortedSet<string>();
            SortedSet<string> failed = new SortedSet<string>();
            SortedSet<string> notHandled = new SortedSet<string>();

            switch (policyResult)
            {
                case PolicyResult.Success:
                    succeeded.Add(policyExpression);
                    break;
                case PolicyResult.Failed:
                    failed.Add(policyExpression);
                    break;
                case PolicyResult.NotHandled:
                default:
                    notHandled.Add(policyExpression);
                    break;
            }

            AuthorizationResult result = new AuthorizationResult(policyResult, succeeded, failed, notHandled);
            accessor.Result = result;

            bool overall = false;
            switch (result.Result)
            {
                case PolicyResult.Success:
                    overall = true;
                    break;
                case PolicyResult.Failed:
                    break;
                case PolicyResult.NotHandled:
                default:
                    if (!failedIfNotHandled)
                        overall = true;
                    break;
            }
            if (overall)
                return;
            switch (failedAction)
            {
                case AuthorizationFailedAction.KeepUnauthorized:
                    return;

                case AuthorizationFailedAction.Return401:
                    context.Result = new HttpUnauthorizedResult();
                    return;

                case AuthorizationFailedAction.CustomHandler:
                    break;
            }

            AuthorizationFailedHandlerAttribute[] handlers;
            switch (context.ActionDescriptor)
            {
                case ControllerActionDescriptor controllerActionDescriptor:
                    handlers = GetCustomHandlers(controllerActionDescriptor);
                    break;
                case CompiledPageActionDescriptor compiledPageActionDescriptor:
                    handlers = GetCustomHandlers(compiledPageActionDescriptor);
                    break;
                default:
                    throw new Exception($"not handled with action descriptor of type {context.ActionDescriptor.GetType().Name}");
            }

            if (handlers != null && handlers.Length > 0)
            {
                IActionResult actionResult = handlers[0].Execute(context.HttpContext, result);
                if (actionResult != null)
                {
                    context.Result = actionResult;
                    return;
                }
                else
                {
                    // not handled
                    throw new Exception($"not handled");
                }
            }
        }

        private AuthorizationFailedHandlerAttribute[] GetCustomHandlers(ControllerActionDescriptor controllerActionDescriptor)
        {
            List<AuthorizationFailedHandlerAttribute> result = new List<AuthorizationFailedHandlerAttribute>();
            MethodInfo method = controllerActionDescriptor.MethodInfo;
            if (method.HasAttribute<AuthorizationFailedHandlerAttribute>(false))
            {
                result.AddRange(method.GetCustomAttributes<AuthorizationFailedHandlerAttribute>(false));
            }
            else
            {
                Type baseType;
                TypeInfo controllerType;
                while (true)
                {
                    controllerType = controllerActionDescriptor.ControllerTypeInfo;
                    if (controllerType.HasAttribute<AuthorizationFailedHandlerAttribute>(false))
                    {
                        result.AddRange(controllerType.GetCustomAttributes<AuthorizationFailedHandlerAttribute>(false));
                        break;
                    }
                    baseType = controllerType.BaseType;
                    if (baseType == null)
                        break;

                    controllerType = baseType.GetTypeInfo();
                }
            }
            return result.ToArray();
        }
        private AuthorizationFailedHandlerAttribute[] GetCustomHandlers(CompiledPageActionDescriptor compiledPageActionDescriptor)
        {
            List<AuthorizationFailedHandlerAttribute> result = new List<AuthorizationFailedHandlerAttribute>();
            HandlerMethodDescriptor methodDescriptor = compiledPageActionDescriptor.HandlerMethods[0];
            bool checkPageModel = true;
            if (methodDescriptor != null)
            {
                MethodInfo method = methodDescriptor.MethodInfo;
                if (method.HasAttribute<AuthorizationFailedHandlerAttribute>(false))
                {
                    result.AddRange(method.GetCustomAttributes<AuthorizationFailedHandlerAttribute>(false));
                    checkPageModel = false;
                }
            }

            if (checkPageModel)
            {
                Type baseType;
                TypeInfo controllerType;
                while (true)
                {
                    controllerType = compiledPageActionDescriptor.ModelTypeInfo;
                    if (controllerType.HasAttribute<AuthorizationFailedHandlerAttribute>(false))
                    {
                        result.AddRange(controllerType.GetCustomAttributes<AuthorizationFailedHandlerAttribute>(false));
                        break;
                    }
                    baseType = controllerType.BaseType;
                    if (baseType == null)
                        break;

                    controllerType = baseType.GetTypeInfo();
                }
            }
            return result.ToArray();
        }
    }
}
