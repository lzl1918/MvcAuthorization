using System;
using System.Collections.Generic;
using System.Text;

namespace AuthorizationCore.Services.Internals
{

    internal sealed class AuthorizationOptions<TUser> : IAuthorizationOptions<TUser>
    {
        private readonly Dictionary<string, object> policies;
        private readonly Dictionary<string, Delegate> objectAccessors;
        private readonly Dictionary<Type, Type> handlers;
        private readonly Func<IServiceProvider, TUser> userAccessor;

        public Dictionary<string, object> Policies => policies;
        public Dictionary<string, Delegate> ObjectAccessors => objectAccessors;
        public Dictionary<Type, Type> Handlers => handlers;
        public Func<IServiceProvider, TUser> UserAccessor => userAccessor;

        public AuthorizationOptions(Dictionary<string, object> policies, Dictionary<string, Delegate> objectAccessors, Dictionary<Type, Type> handlers, Func<IServiceProvider, TUser> userAccessor)
        {
            this.policies = policies;
            this.objectAccessors = objectAccessors;
            this.handlers = handlers;
            this.userAccessor = userAccessor;
        }
    }
}
