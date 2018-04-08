using System;
using System.Collections.Generic;
using System.Text;

namespace AuthorizationCore.Services.Internals
{
    internal sealed class AuthorizationOptionsBuilder<TUser> : IAuthorizationOptionsBuilder<TUser>
    {
        private Dictionary<string, object> policies;
        private Dictionary<string, Delegate> objectAccessors;
        private Dictionary<Type, Type> handlers;
        private Func<IServiceProvider, TUser> userAccessor;
        public Func<IServiceProvider, TUser> UserAccessor { get => userAccessor; set => userAccessor = value; }

        internal AuthorizationOptionsBuilder()
        {
            policies = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);
            objectAccessors = new Dictionary<string, Delegate>(StringComparer.CurrentCultureIgnoreCase);
            handlers = new Dictionary<Type, Type>();
        }

        public IAuthorizationOptionsBuilder<TUser> AddHandler<TPolicy, THandler>()
            where TPolicy : IPolicy<TUser>
            where THandler : IPolicyHandler<TUser, TPolicy>
        {
            Type policyType = typeof(TPolicy);
            Type handlerType = typeof(THandler);
            if (handlers.ContainsKey(policyType))
                throw new Exception($"Handler of target policy type {policyType.Name} already exists");
            handlers[policyType] = handlerType;
            return this;
        }

        public IAuthorizationOptionsBuilder<TUser> AddPolicy(IPolicy<TUser> policy, string name)
        {
            if (policy == null)
                throw new ArgumentNullException(nameof(policy));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("value cannot be null or whitespace", nameof(name));

            if (policies.ContainsKey(name))
                throw new Exception($"Policy with the name {name} already exists");
            policies[name] = policy;

            return this;
        }

        internal IAuthorizationOptions<TUser> Build()
        {
            if (userAccessor == null)
                throw new Exception("user accessor is not set");
            return new AuthorizationOptions<TUser>(policies, objectAccessors, handlers, userAccessor);
        }

        public IAuthorizationOptionsBuilder<TUser> AddPolicy<TObject>(IPolicy<TUser, TObject> policy, Func<IServiceProvider, TObject> accessor, string name)
        {
            if (policy == null)
                throw new ArgumentNullException(nameof(policy));
            if (accessor == null)
                throw new ArgumentNullException(nameof(accessor));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("value cannot be null or whitespace", nameof(name));

            if (policies.ContainsKey(name))
                throw new Exception($"Policy with the name {name} already exists");
            policies[name] = policy;
            objectAccessors[name] = accessor;
            return this;
        }

        public IAuthorizationOptionsBuilder<TUser> AddHandler<TObject, TPolicy, THandler>()
            where TPolicy : IPolicy<TUser, TObject>
            where THandler : IPolicyHandler<TUser, TObject, TPolicy>
        {
            Type policyType = typeof(TPolicy);
            Type handlerType = typeof(THandler);
            if (handlers.ContainsKey(policyType))
                throw new Exception($"Handler of target policy type {policyType.Name} already exists");
            handlers[policyType] = handlerType;
            return this;
        }
    }
}
