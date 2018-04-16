using AuthorizationCore.Attributes;
using AuthorizationCore.Internal.Helpers;
using Hake.Extension.Cache;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AuthorizationCore.Internals.Services
{
    internal static class CacheFallbackCollection
    {
        private static readonly Type TaskType = typeof(Task<int>).GetGenericTypeDefinition();
        private static readonly Type BaseTaskType = typeof(Task);
        private static readonly Type IActionResultType = typeof(IActionResult);
        private static readonly Type VoidType = typeof(void);

        internal static readonly CacheFallBack<Type, InvokeMethodInfo> HandlerInvokeMethodFallback = key =>
        {
            MethodInfo method = key.GetMethod("InvokeAsync", BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance);
            if (method != null &&
                TryValidateInvokeReturnType(method, out InvokeMethodInfo methodInfo))
            {
                return RetrivationResult<InvokeMethodInfo>.Create(methodInfo);
            }
            method = key.GetMethod("Invoke", BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance);
            if (method != null &&
                TryValidateInvokeReturnType(method, out methodInfo))
            {
                return RetrivationResult<InvokeMethodInfo>.Create(methodInfo);
            }
            return RetrivationResult<InvokeMethodInfo>.Create(null);
        };
        private static bool TryValidateInvokeReturnType(MethodInfo method, out InvokeMethodInfo methodInfo)
        {
            Type returnType = method.ReturnType;
            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition().Equals(TaskType))
            {
                Type innerType = returnType.GetGenericArguments()[0];
                if (IActionResultType.IsAssignableFrom(innerType))
                {
                    MethodInfo getAwaiterMethod = returnType.GetMethod("GetAwaiter", BindingFlags.Public | BindingFlags.Instance);
                    Type awaiterType = getAwaiterMethod.ReturnType;
                    MethodInfo getResultMethod = awaiterType.GetMethod("GetResult", BindingFlags.Public | BindingFlags.Instance);
                    methodInfo = new InvokeMethodInfo(method, InvokeMethodReturnType.TaskWithIActionResult, getAwaiterMethod, getResultMethod);
                    return true;
                }
            }
            else if (BaseTaskType.IsAssignableFrom(returnType))
            {
                methodInfo = new InvokeMethodInfo(method, InvokeMethodReturnType.Task, null, null);
                return true;
            }
            else if (returnType.Equals(VoidType))
            {
                methodInfo = new InvokeMethodInfo(method, InvokeMethodReturnType.Void, null, null);
                return true;
            }
            else if (IActionResultType.IsAssignableFrom(returnType))
            {
                methodInfo = new InvokeMethodInfo(method, InvokeMethodReturnType.IActionResult, null, null);
                return true;
            }
            methodInfo = null;
            return false;
        }

        internal static readonly CacheFallBack<ControllerActionDescriptor, AuthorizationDeclarationInfo> ControllerAuthorizationDeclarationFallback = key =>
        {
            AuthorizationRequiredAttribute attribute = key.MethodInfo.GetCustomAttributes<AuthorizationRequiredAttribute>(false).FirstOrDefault();
            if (attribute != null)
            {
                string policyExpression = attribute.policyExpression;
                AuthorizationFailedAction failedAction = attribute.failedAction;
                bool failedIfNotHandled = attribute.failedIfNotHandled;
                AuthorizationFailedHandlerAttribute handlerAttribute = null;
                if (failedAction == AuthorizationFailedAction.CustomHandler)
                    handlerAttribute = GetCustomHandlerOrDefault(key);
                return RetrivationResult<AuthorizationDeclarationInfo>.Create(new AuthorizationDeclarationInfo(AuthorizationDeclaration.Action, policyExpression, failedAction, failedIfNotHandled, handlerAttribute));
            }

            attribute = key.ControllerTypeInfo.GetCustomAttributes<AuthorizationRequiredAttribute>(true).FirstOrDefault();
            if (attribute == null)
                return RetrivationResult<AuthorizationDeclarationInfo>.Create(new AuthorizationDeclarationInfo(AuthorizationDeclaration.No, null, AuthorizationFailedAction.KeepUnauthorized, false, null));

            {
                string policyExpression = attribute.policyExpression;
                AuthorizationFailedAction failedAction = attribute.failedAction;
                bool failedIfNotHandled = attribute.failedIfNotHandled;
                AuthorizationFailedHandlerAttribute handlerAttribute = null;
                if (failedAction == AuthorizationFailedAction.CustomHandler)
                    handlerAttribute = GetCustomHandlerOrDefault(key);
                return RetrivationResult<AuthorizationDeclarationInfo>.Create(new AuthorizationDeclarationInfo(AuthorizationDeclaration.Controller, policyExpression, failedAction, failedIfNotHandled, handlerAttribute));
            }
        };

        internal static readonly CacheFallBack<CompiledPageActionDescriptor, AuthorizationDeclarationInfo> PageAuthorizationDeclarationFallback = key =>
        {
            HandlerMethodDescriptor handler = key.HandlerMethods[0];
            if (handler != null)
            {
                AuthorizationRequiredAttribute attribute = handler.MethodInfo.GetCustomAttributes<AuthorizationRequiredAttribute>(false).FirstOrDefault();
                if (attribute != null)
                {
                    string policyExpression = attribute.policyExpression;
                    AuthorizationFailedAction failedAction = attribute.failedAction;
                    bool failedIfNotHandled = attribute.failedIfNotHandled;
                    AuthorizationFailedHandlerAttribute handlerAttribute = null;
                    if (failedAction == AuthorizationFailedAction.CustomHandler)
                        handlerAttribute = GetCustomHandlerOrDefault(key);
                    return RetrivationResult<AuthorizationDeclarationInfo>.Create(new AuthorizationDeclarationInfo(AuthorizationDeclaration.HandlerMethod, policyExpression, failedAction, failedIfNotHandled, handlerAttribute));
                }
            }
            {
                AuthorizationRequiredAttribute attribute = key.ModelTypeInfo.GetCustomAttributes<AuthorizationRequiredAttribute>(true).FirstOrDefault();
                if (attribute == null)
                    return RetrivationResult<AuthorizationDeclarationInfo>.Create(new AuthorizationDeclarationInfo(AuthorizationDeclaration.No, null, AuthorizationFailedAction.KeepUnauthorized, false, null));

                {
                    string policyExpression = attribute.policyExpression;
                    AuthorizationFailedAction failedAction = attribute.failedAction;
                    bool failedIfNotHandled = attribute.failedIfNotHandled;
                    AuthorizationFailedHandlerAttribute handlerAttribute = null;
                    if (failedAction == AuthorizationFailedAction.CustomHandler)
                        handlerAttribute = GetCustomHandlerOrDefault(key);
                    return RetrivationResult<AuthorizationDeclarationInfo>.Create(new AuthorizationDeclarationInfo(AuthorizationDeclaration.PageModel, policyExpression, failedAction, failedIfNotHandled, handlerAttribute));
                }
            }
        };
        private static AuthorizationFailedHandlerAttribute GetCustomHandlerOrDefault(ControllerActionDescriptor controllerActionDescriptor)
        {
            AuthorizationFailedHandlerAttribute result = controllerActionDescriptor
                                                            .MethodInfo
                                                            .GetCustomAttributes<AuthorizationFailedHandlerAttribute>(false)
                                                            .FirstOrDefault();
            if (result != null)
                return result;

            result = controllerActionDescriptor.ControllerTypeInfo
                                               .GetCustomAttributes<AuthorizationFailedHandlerAttribute>(true)
                                               .FirstOrDefault();
            return result;
        }
        private static AuthorizationFailedHandlerAttribute GetCustomHandlerOrDefault(CompiledPageActionDescriptor compiledPageActionDescriptor)
        {
            AuthorizationFailedHandlerAttribute result;
            HandlerMethodDescriptor methodDescriptor = compiledPageActionDescriptor.HandlerMethods[0];
            if (methodDescriptor != null)
            {
                result = methodDescriptor.MethodInfo
                                         .GetCustomAttributes<AuthorizationFailedHandlerAttribute>(false)
                                         .FirstOrDefault();
                if (result != null)
                    return result;
            }

            result = compiledPageActionDescriptor.ModelTypeInfo
                                                 .GetCustomAttributes<AuthorizationFailedHandlerAttribute>(true)
                                                 .FirstOrDefault();
            return result;
        }
    }
}
