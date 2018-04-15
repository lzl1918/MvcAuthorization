using AuthorizationCore.Attributes;
using Hake.Extension.Cache;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AuthorizationCore.Services.Internals
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
                Type type = attribute.GetType();
                string policyExpression = (string)type.GetField("policyExpression").GetValue(attribute);
                AuthorizationFailedAction failedAction = (AuthorizationFailedAction)type.GetField("failedAction").GetValue(attribute);
                bool failedIfNotHandled = (bool)type.GetField("failedIfNotHandled").GetValue(attribute);
                return RetrivationResult<AuthorizationDeclarationInfo>.Create(new AuthorizationDeclarationInfo(AuthorizationDeclaration.Action, policyExpression, failedAction, failedIfNotHandled));
            }

            attribute = key.ControllerTypeInfo.GetCustomAttributes<AuthorizationRequiredAttribute>(true).FirstOrDefault();
            if (attribute == null)
                return RetrivationResult<AuthorizationDeclarationInfo>.Create(new AuthorizationDeclarationInfo(AuthorizationDeclaration.No, null, AuthorizationFailedAction.KeepUnauthorized, false));

            {
                Type type = attribute.GetType();
                string policyExpression = (string)type.GetField("policyExpression").GetValue(attribute);
                AuthorizationFailedAction failedAction = (AuthorizationFailedAction)type.GetField("failedAction").GetValue(attribute);
                bool failedIfNotHandled = (bool)type.GetField("failedIfNotHandled").GetValue(attribute);
                return RetrivationResult<AuthorizationDeclarationInfo>.Create(new AuthorizationDeclarationInfo(AuthorizationDeclaration.Controller, policyExpression, failedAction, failedIfNotHandled));
            }
        };

        internal static readonly CacheFallBack<CompiledPageActionDescriptor, AuthorizationDeclarationInfo> PageAuthorizationDeclarationFallback = key =>
        {
            HandlerMethodDescriptor handler = key.HandlerMethods[0];
            if (handler != null)
            {
                object attribute = handler.MethodInfo.GetCustomAttributes<AuthorizationRequiredAttribute>(false).FirstOrDefault();
                if (attribute != null)
                {
                    Type type = attribute.GetType();
                    string policyExpression = (string)type.GetField("policyExpression").GetValue(attribute);
                    AuthorizationFailedAction failedAction = (AuthorizationFailedAction)type.GetField("failedAction").GetValue(attribute);
                    bool failedIfNotHandled = (bool)type.GetField("failedIfNotHandled").GetValue(attribute);
                    return RetrivationResult<AuthorizationDeclarationInfo>.Create(new AuthorizationDeclarationInfo(AuthorizationDeclaration.HandlerMethod, policyExpression, failedAction, failedIfNotHandled));
                }
            }
            {
                object attribute = key.ModelTypeInfo.GetCustomAttributes<AuthorizationRequiredAttribute>(true).FirstOrDefault();
                if (attribute == null)
                    return RetrivationResult<AuthorizationDeclarationInfo>.Create(new AuthorizationDeclarationInfo(AuthorizationDeclaration.No, null, AuthorizationFailedAction.KeepUnauthorized, false));

                {
                    Type type = attribute.GetType();
                    string policyExpression = (string)type.GetField("policyExpression").GetValue(attribute);
                    AuthorizationFailedAction failedAction = (AuthorizationFailedAction)type.GetField("failedAction").GetValue(attribute);
                    bool failedIfNotHandled = (bool)type.GetField("failedIfNotHandled").GetValue(attribute);
                    return RetrivationResult<AuthorizationDeclarationInfo>.Create(new AuthorizationDeclarationInfo(AuthorizationDeclaration.PageModel, policyExpression, failedAction, failedIfNotHandled));
                }
            }
        };
    }
}
