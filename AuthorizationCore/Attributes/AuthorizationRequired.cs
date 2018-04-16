using AuthorizationCore.Expressions;
using AuthorizationCore.Internal.Helpers;
using AuthorizationCore.Internals;
using AuthorizationCore.Internals.Responses;
using AuthorizationCore.Internals.Services;
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
        private static readonly Type AuthorizationServiceType = typeof(IAuthorizationService<int>).GetGenericTypeDefinition();
        internal readonly string policyExpression;
        internal readonly AuthorizationFailedAction failedAction;
        internal readonly bool failedIfNotHandled;
        private readonly Type userType;
        private readonly Type serviceType;
        private readonly MethodInfo tryAuthorizeMethod;

        public AuthorizationRequiredAttribute(string expression, Type userType, AuthorizationFailedAction failedAction = AuthorizationFailedAction.Return401, bool failedIfNotHandled = true)
        {
            if (userType == null)
                throw new ArgumentNullException(nameof(userType));
            if (string.IsNullOrWhiteSpace(expression))
                throw new Exception();

            this.policyExpression = expression;
            this.userType = userType;
            this.failedAction = failedAction;
            this.failedIfNotHandled = failedIfNotHandled;
            serviceType = AuthorizationServiceType.MakeGenericType(userType);
            tryAuthorizeMethod = serviceType.GetMethod("TryAuthorize", BindingFlags.Public | BindingFlags.Instance);
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            IServiceProvider services = context.HttpContext.RequestServices;
            object authorization = services.GetRequiredService(serviceType);
            IAuthorizationResultAccessor accessor = services.GetRequiredService<IAuthorizationResultAccessor>();
            PolicyResult policyResult = (PolicyResult)tryAuthorizeMethod.Invoke(authorization, new object[] { policyExpression });
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

            PolicyOnlyExpression exp = new PolicyOnlyExpression(policyExpression, policyResult);
            PolicyExpressionRoot root = new PolicyExpressionRoot(exp);
            AuthorizationResult result = new AuthorizationResult(root);
            if (accessor.Result == null)
                accessor.Result = result;
            else
            {
                result.CombineAsAnd(accessor.Result);
                accessor.Result = result;
            }

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

            // custom handler
            IAuthorizationDeclarationCache cache = services.GetRequiredService<IAuthorizationDeclarationCache>();
            AuthorizationDeclarationInfo info = null;
            switch (context.ActionDescriptor)
            {
                case ControllerActionDescriptor controllerActionDescriptor:
                    info = cache.Get(controllerActionDescriptor);
                    break;
                case CompiledPageActionDescriptor compiledPageActionDescriptor:
                    info = cache.Get(compiledPageActionDescriptor);
                    break;
                default:
                    throw new Exception($"not handled with action descriptor of type {context.ActionDescriptor.GetType().Name}");
            }

            if (info != null &&
                info.Declaration != AuthorizationDeclaration.No &&
                info.FailedAction == AuthorizationFailedAction.CustomHandler &&
                info.FailedHandler != null)
            {
                IActionResult actionResult = info.FailedHandler.Execute(context.HttpContext, result);
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
            throw new Exception($"no handler set for type {context.ActionDescriptor.GetType().Name}");
        }


    }
}
