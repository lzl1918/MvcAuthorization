using AuthorizationCore.Expressions;
using System.Collections.Generic;

namespace AuthorizationCore
{
    public interface IAuthorizationResult
    {
        IReadOnlyCollection<string> SucceededPolicies { get; }
        IReadOnlyCollection<string> FailedPolicies { get; }
        IReadOnlyCollection<string> NotHandledPolicies { get; }
        PolicyResult this[string policy] { get; }
        PolicyResult Result { get; }
        IPolicyExpressionRoot Expression { get; }
        bool IsAuthorizationRequired { get; }
    }

    public static class AuthorizationResultExtensions
    {
        public static bool Succeeded(this IAuthorizationResult result, bool failedIfNotHandled = true)
        {
            switch (result.Result)
            {
                case PolicyResult.Success:
                    return true;
                case PolicyResult.Failed:
                    return false;
                case PolicyResult.NotHandled:
                default:
                    return !failedIfNotHandled;
            }
        }
    }
}
