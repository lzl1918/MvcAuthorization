﻿using AuthorizationCore.Attributes;
using AuthorizationCore.Internals;
using AuthorizationCore.Internals.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AuthorizationCore
{
    public static class AuthorizationExtensions
    {
        public static IServiceCollection AddMvcAuthorization<TUser>(this IServiceCollection services, Action<IAuthorizationOptionsBuilder<TUser>> optionsBuilder, int cacheCapacity = 100)
        {
            AuthorizationOptionsBuilder<TUser> builder = new AuthorizationOptionsBuilder<TUser>();
            optionsBuilder(builder);
            services.AddSingleton<IAuthorizationOptions<TUser>>(builder.Build());
            services.AddScoped<IAuthorizationService<TUser>, AuthorizationService<TUser>>();
            services.AddSingleton<IHandlerInvokeMethodCache>(provider =>
            {
                return new HandlerInvokeMethodCache(cacheCapacity);
            });
            services.AddSingleton<IAuthorizationDeclarationCache>(provider =>
            {
                return new AuthorizationDeclarationCache(cacheCapacity);
            });
            services.AddScoped<IAuthorizationResultAccessor, AuthorizationResultAccessor>();
            services.AddScoped<IAuthorizationResult>(provider =>
            {
                IAuthorizationResultAccessor accessor = provider.GetRequiredService<IAuthorizationResultAccessor>();
                if (accessor.Result == null)
                    accessor.Result = AuthorizationResult.Empty;
                return accessor.Result;
            });
            return services;
        }
    }
}
