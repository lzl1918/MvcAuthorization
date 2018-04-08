using AuthorizationCore.Services;
using AuthorizationCore.Services.Internals;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace AuthorizationCore
{
    public static class AuthorizationExtensions
    {
        public static IServiceCollection AddMvcAuthorization<TUser>(this IServiceCollection services, Action<IAuthorizationOptionsBuilder<TUser>> optionsBuilder)
        {
            AuthorizationOptionsBuilder<TUser> builder = new AuthorizationOptionsBuilder<TUser>();
            optionsBuilder(builder);
            services.AddSingleton<IAuthorizationOptions<TUser>>(builder.Build());
            services.AddScoped<IAuthorizationService<TUser>, AuthorizationService<TUser>>();
            return services;
        }
    }
}
