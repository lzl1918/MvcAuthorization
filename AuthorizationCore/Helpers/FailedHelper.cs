using AuthorizationCore.Services;
using AuthorizationCore.Services.Internals;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AuthorizationCore.Helpers
{
    internal static class FailedHelper
    {
        #region CUSTOM_HANDLER_EXECUTE
        private static object[] PrepareHandlerMethodParameters(MethodInfo method, IServiceProvider services, HttpContext httpContext, IAuthorizationResult authorizationResult)
        {
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length <= 0)
                return null;

            object[] result = new object[parameters.Length];
            object value;
            int i = 0;
            foreach (ParameterInfo parameter in parameters)
            {
                if (parameter.ParameterType.Equals(typeof(HttpContext)))
                    result[i] = httpContext;
                else if (parameter.ParameterType.Equals(typeof(IAuthorizationResult)))
                    result[i] = authorizationResult;
                else
                {
                    value = services.GetService(parameter.ParameterType);
                    if (value == null)
                    {
                        if (parameter.HasDefaultValue)
                            value = parameter.DefaultValue;
                        else
                            value = parameter.ParameterType.IsValueType ? Activator.CreateInstance(parameter.ParameterType) : null;
                    }
                    result[i] = value;
                }
                i++;
            }
            return result;
        }

        internal static IActionResult ExecuteHandler(Type handler, object[] constructParameters, HttpContext httpContext, IAuthorizationResult authorizationResult)
        {
            IServiceProvider services = httpContext.RequestServices;
            IHandlerInvokeMethodCache cache = services.GetRequiredService<IHandlerInvokeMethodCache>();
            InvokeMethodInfo methodInfo = cache.Get(handler);
            if (methodInfo != null)
                return ExecuteMethod(methodInfo);

            return null;

            IActionResult ExecuteMethod(InvokeMethodInfo info)
            {
                try
                {
                    MethodInfo method = info.Method;
                    object handler_instance = ActivatorUtilities.CreateInstance(services, handler, constructParameters);
                    object invoke_result = method.Invoke(handler_instance, PrepareHandlerMethodParameters(method, services, httpContext, authorizationResult));
                    switch (info.ReturnType)
                    {
                        case InvokeMethodReturnType.Void:
                            return null;

                        case InvokeMethodReturnType.Task:
                            {
                                Task result = (Task)invoke_result;
                                if (result == null)
                                    return null;
                                if (result.Status == TaskStatus.WaitingToRun || result.Status == TaskStatus.Created)
                                    result.Start();
                                result.Wait();
                                return null;
                            }

                        case InvokeMethodReturnType.TaskWithIActionResult:
                            {
                                object awaiter = info.GetAwaiter.Invoke(invoke_result, null);
                                IActionResult result = (IActionResult)info.GetResult.Invoke(awaiter, null);
                                if (result == null)
                                    return null;
                                return result;
                            }

                        case InvokeMethodReturnType.IActionResult:
                        default:
                            {
                                IActionResult result = (IActionResult)invoke_result;
                                if (result == null)
                                    return null;
                                return result;
                            }

                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    return null;
                }
            }
        }
        #endregion CUSTOM_HANDLER_EXECUTE
    }
}
