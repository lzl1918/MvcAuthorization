using System.Collections.Generic;

namespace AuthorizationCore.Services
{
    public interface IAuthorizationResult
    {
        IReadOnlyCollection<string> SucceededPolicies { get; }
        IReadOnlyCollection<string> FailedPolicies { get; }
        IReadOnlyCollection<string> NotHandledPolicies { get; }
        PolicyResult this[string policy] { get; }
        PolicyResult Result { get; }
    }
}
