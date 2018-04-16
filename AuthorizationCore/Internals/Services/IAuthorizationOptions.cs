using System;
using System.Collections.Generic;
using System.Text;

namespace AuthorizationCore.Internals.Services
{

    internal interface IAuthorizationOptions<TUser>
    {
        Dictionary<string, object> Policies { get; }
        Dictionary<string, Delegate> ObjectAccessors { get; }
        Dictionary<Type, Type> Handlers { get; }
        Func<IServiceProvider, TUser> UserAccessor { get; }
    }
}
