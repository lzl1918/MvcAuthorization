using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Reflection;
using System.Linq;

namespace AuthorizationCore.Services.Internals
{
    internal sealed class AuthorizationService<TUser> : IAuthorizationService<TUser>
    {
        private readonly IAuthorizationOptions<TUser> options;
        private readonly IServiceProvider services;
        private readonly static Type IPolicyBaseType = typeof(IPolicy<TUser>);
        private readonly static Type UserType = typeof(TUser);
        private readonly static Type IObjectPolicyBaseType = typeof(IPolicy<TUser, int>).GetGenericTypeDefinition();

        public AuthorizationService(IAuthorizationOptions<TUser> options, IServiceProvider services)
        {
            this.options = options;
            this.services = services;
        }
        public PolicyResult TryAuthorize(string policy)
        {
            if (!options.Policies.TryGetValue(policy, out object policyObject))
                return PolicyResult.NotHandled;

            Type objectType = policyObject.GetType();
            if (IPolicyBaseType.IsAssignableFrom(objectType))
                return TryAuthorizeUserOnlyPolicy((IPolicy<TUser>)policyObject);

            if (options.ObjectAccessors.TryGetValue(policy, out Delegate accessor))
                return TryAuthroizeTargetPolicy(policyObject, accessor);

            return PolicyResult.NotHandled;
        }
        private PolicyResult TryAuthorizeUserOnlyPolicy(IPolicy<TUser> policy)
        {
            Type objectType = policy.GetType();
            Type currentType = objectType;
            Type baseType;
            Dictionary<Type, Type> handlers = options.Handlers;
            Type handlerType = null;
            while (true)
            {
                if (handlers.TryGetValue(currentType, out handlerType))
                    break;
                baseType = currentType.BaseType;
                if (IPolicyBaseType.IsAssignableFrom(baseType))
                    currentType = baseType;
                else
                    break;
            }
            if (handlerType == null)
            {
                foreach (Type interfaceType in objectType.GetInterfaces())
                {
                    if (IPolicyBaseType.IsAssignableFrom(interfaceType) &&
                        handlers.TryGetValue(interfaceType, out handlerType))
                        break;
                }
            }
            if (handlerType == null)
                return PolicyResult.NotHandled;

            object handler = ActivatorUtilities.CreateInstance(services, handlerType);
            TUser user = options.UserAccessor(services);
            MethodInfo authorizeMethod = handlerType.GetMethod("OnAuthorization");
            return (PolicyResult)authorizeMethod.Invoke(handler, new object[] { user, policy });
        }
        private PolicyResult TryAuthroizeTargetPolicy(object policy, Delegate accessor)
        {
            Type objectType = policy.GetType();
            Type interfaceType = objectType.GetInterfaces().FirstOrDefault(type => type.GetGenericTypeDefinition().Equals(IObjectPolicyBaseType));
            if (interfaceType == null)
                return PolicyResult.NotHandled;
            Type[] genericArgs = interfaceType.GetGenericArguments();
            if (!genericArgs[0].Equals(UserType))
                return PolicyResult.NotHandled;
            Type targetType = genericArgs[1];

            Type currentType = objectType;
            Type baseType;
            Dictionary<Type, Type> handlers = options.Handlers;
            Type handlerType = null;
            while (true)
            {
                if (handlers.TryGetValue(currentType, out handlerType))
                    break;
                baseType = currentType.BaseType;
                if (interfaceType.IsAssignableFrom(baseType))
                    currentType = baseType;
                else
                    break;
            }
            if (handlerType == null)
            {
                foreach (Type typeInterface in objectType.GetInterfaces())
                {
                    if (interfaceType.IsAssignableFrom(typeInterface) &&
                        handlers.TryGetValue(typeInterface, out handlerType))
                        break;
                }
            }
            if (handlerType == null)
                return PolicyResult.NotHandled;

            object handler = ActivatorUtilities.CreateInstance(services, handlerType);
            TUser user = options.UserAccessor(services);
            object targetObject = accessor.DynamicInvoke(services);
            MethodInfo authorizeMethod = handlerType.GetMethod("OnAuthorization");
            return (PolicyResult)authorizeMethod.Invoke(handler, new object[] { user, targetObject, policy });
        }
    }
}
