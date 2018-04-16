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
        private ICache<ControllerActionDescriptor, AuthorizationDeclarationInfo> mvcCache;
        private ICache<CompiledPageActionDescriptor, AuthorizationDeclarationInfo> pageCache;

        public AuthorizationDeclarationCache(int capacity)
        {
            mvcCache = new Cache<ControllerActionDescriptor, AuthorizationDeclarationInfo>(capacity, ComparerCollection.ControllerActionDescriptorComparer, ComparerCollection.ControllerActionDescriptorEqualityComparer);
            pageCache = new Cache<CompiledPageActionDescriptor, AuthorizationDeclarationInfo>(capacity, ComparerCollection.CompiledPageActionDescriptorComparer, ComparerCollection.CompiledPageActionDescriptorEqualityComparer);
        }

        public AuthorizationDeclarationInfo Get(ControllerActionDescriptor descriptor)
        {
            return mvcCache.Get(descriptor, CacheFallbackCollection.ControllerAuthorizationDeclarationFallback);
        }

        public AuthorizationDeclarationInfo Get(CompiledPageActionDescriptor descriptor)
        {
            return pageCache.Get(descriptor, CacheFallbackCollection.PageAuthorizationDeclarationFallback);
        }
    }
    internal interface IAuthorizationDeclarationCache
    {
        AuthorizationDeclarationInfo Get(ControllerActionDescriptor descriptor);
        AuthorizationDeclarationInfo Get(CompiledPageActionDescriptor descriptor);
    }
}
