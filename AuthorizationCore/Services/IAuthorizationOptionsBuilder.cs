using System;

namespace AuthorizationCore.Services
{
    public interface IAuthorizationOptionsBuilder<TUser>
    {
        IAuthorizationOptionsBuilder<TUser> AddPolicy(IPolicy<TUser> policy, string name);
        IAuthorizationOptionsBuilder<TUser> AddPolicy<TObject>(IPolicy<TUser, TObject> policy, Func<IServiceProvider, TObject> accessor, string name);
        IAuthorizationOptionsBuilder<TUser> AddHandler<TPolicy, THandler>() where THandler : IPolicyHandler<TUser, TPolicy> where TPolicy : IPolicy<TUser>;
        IAuthorizationOptionsBuilder<TUser> AddHandler<TObject, TPolicy, THandler>() where THandler : IPolicyHandler<TUser, TObject, TPolicy> where TPolicy : IPolicy<TUser, TObject>;

        Func<IServiceProvider, TUser> UserAccessor { get; set; }
    }

}
