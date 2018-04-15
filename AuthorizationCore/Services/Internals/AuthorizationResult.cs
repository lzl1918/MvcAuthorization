using System.Collections.Generic;

namespace AuthorizationCore.Services.Internals
{
    internal sealed class AuthorizationResult : IAuthorizationResult
    {
        private readonly SortedSet<string> succeededPolicies;
        private readonly SortedSet<string> failedPolicies;
        private readonly SortedSet<string> notHandledPolices;
        private readonly Dictionary<string, PolicyResult> policyResults;
        private readonly PolicyResult result;

        public PolicyResult this[string policy] => policyResults[policy];

        public IReadOnlyCollection<string> SucceededPolicies => succeededPolicies;
        public IReadOnlyCollection<string> FailedPolicies => failedPolicies;
        public IReadOnlyCollection<string> NotHandledPolicies => notHandledPolices;
        public PolicyResult Result => result;


        public AuthorizationResult(PolicyResult result, SortedSet<string> succeededPolicies, SortedSet<string> failedPolicies, SortedSet<string> notHandledPolices)
        {
            this.result = result;
            this.succeededPolicies = succeededPolicies;
            this.failedPolicies = failedPolicies;
            this.notHandledPolices = notHandledPolices;
            policyResults = new Dictionary<string, PolicyResult>();
            foreach (string policy in succeededPolicies)
            {
                policyResults.Add(policy, PolicyResult.Success);
            }
            foreach (string policy in failedPolicies)
            {
                policyResults.Add(policy, PolicyResult.Failed);
            }
            foreach (string policy in notHandledPolices)
            {
                policyResults.Add(policy, PolicyResult.NotHandled);
            }
        }

        public static IAuthorizationResult Empty { get; } = new AuthorizationResult(PolicyResult.NotHandled, new SortedSet<string>(), new SortedSet<string>(), new SortedSet<string>());

    }
}
