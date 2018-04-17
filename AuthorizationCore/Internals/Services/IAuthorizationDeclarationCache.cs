using AuthorizationCore.Attributes;
using Hake.Extension.Cache;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

namespace AuthorizationCore.Internals.Services
{
    internal enum AuthorizationDeclaration
    {
        No = 0,
        Action = 1,
        HandlerMethod = 1,

        Controller = 2,
        PageModel = 2
    }

    internal sealed class AuthorizationDeclarationInfo
    {
        public AuthorizationDeclaration Declaration { get; }
        public string PolicyExpression { get; }
        public AuthorizationFailedAction FailedAction { get; }
        public bool FailedIfNotHandled { get; }
        public AuthorizationFailedHandlerAttribute FailedHandler { get; }

        public AuthorizationDeclarationInfo(AuthorizationDeclaration declaration, string policyExpression, AuthorizationFailedAction failedAction, bool failedIfNotHandled, AuthorizationFailedHandlerAttribute failedHandler)
        {
            Declaration = declaration;
            PolicyExpression = policyExpression;
            FailedAction = failedAction;
            FailedIfNotHandled = failedIfNotHandled;
            FailedHandler = failedHandler;
        }
    }
    internal sealed class AuthorizationDeclarationCache : IAuthorizationDeclarationCache
    {
        private ICache<string, AuthorizationDeclarationInfo> mvcCache;
        private ICache<string, AuthorizationDeclarationInfo> pageCache;
        private object mvcLocker = new object();
        private object pageLocker = new object();

        public AuthorizationDeclarationCache(int capacity)
        {
            mvcCache = new Cache<string, AuthorizationDeclarationInfo>(capacity);
            pageCache = new Cache<string, AuthorizationDeclarationInfo>(capacity);
        }

        public AuthorizationDeclarationInfo Get(ControllerActionDescriptor descriptor)
        {
            lock (mvcLocker)
            {
                string key = $"{descriptor.ControllerTypeInfo.FullName}.{descriptor.MethodInfo.Name}";
                return mvcCache.Get(key, k => CacheFallbackCollection.ControllerAuthorizationDeclarationFallback(descriptor));
            }
        }

        public AuthorizationDeclarationInfo Get(CompiledPageActionDescriptor descriptor)
        {
            lock (pageLocker)
            {
                string key = $"{descriptor.ModelTypeInfo.FullName}.{descriptor.HandlerMethods[0].MethodInfo.Name}";
                return pageCache.Get(key, k => CacheFallbackCollection.PageAuthorizationDeclarationFallback(descriptor));
            }
        }
    }
    internal interface IAuthorizationDeclarationCache
    {
        AuthorizationDeclarationInfo Get(ControllerActionDescriptor descriptor);
        AuthorizationDeclarationInfo Get(CompiledPageActionDescriptor descriptor);
    }
}
