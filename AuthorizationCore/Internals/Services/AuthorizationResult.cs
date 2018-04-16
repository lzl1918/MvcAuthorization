using AuthorizationCore.Expressions;
using System.Collections.Generic;

namespace AuthorizationCore.Internals.Services
{
    internal sealed class AuthorizationResult : IAuthorizationResult
    {
        private readonly SortedSet<string> succeededPolicies;
        private readonly SortedSet<string> failedPolicies;
        private readonly SortedSet<string> notHandledPolicies;
        private readonly Dictionary<string, PolicyResult> policyResults;
        private IPolicyExpressionRootCombine expression;
        private PolicyResult result;

        public PolicyResult this[string policy] => policyResults[policy];

        public IReadOnlyCollection<string> SucceededPolicies => succeededPolicies;
        public IReadOnlyCollection<string> FailedPolicies => failedPolicies;
        public IReadOnlyCollection<string> NotHandledPolicies => notHandledPolicies;
        public PolicyResult Result => result;
        public IPolicyExpressionRoot Expression => expression;
        public bool IsAuthorizationRequired => result != PolicyResult.NotHandled;


        internal AuthorizationResult(IPolicyExpressionRootCombine expression)
        {
            this.expression = expression;
            this.result = expression.Result;
            succeededPolicies = new SortedSet<string>();
            failedPolicies = new SortedSet<string>();
            notHandledPolicies = new SortedSet<string>();
            policyResults = new Dictionary<string, PolicyResult>();

            PolicyResult result;
            foreach (IPolicyOnlyExpression exp in expression)
            {
                if (policyResults.TryGetValue(exp.Policy, out result))
                {
                    switch (result)
                    {
                        case PolicyResult.Success:
                        case PolicyResult.Failed:
                            // keep
                            break;
                        case PolicyResult.NotHandled:
                        default:
                            // overwrite
                            if (exp.Result != PolicyResult.NotHandled)
                                policyResults[exp.Policy] = exp.Result;
                            break;
                    }
                }
                else
                {
                    policyResults.Add(exp.Policy, exp.Result);
                }
            }
            foreach (var pair in policyResults)
            {
                switch (pair.Value)
                {
                    case PolicyResult.Success:
                        succeededPolicies.Add(pair.Key);
                        break;
                    case PolicyResult.Failed:
                        failedPolicies.Add(pair.Key);
                        break;
                    case PolicyResult.NotHandled:
                    default:
                        notHandledPolicies.Add(pair.Key);
                        break;
                }
            }
        }

        // create a empty result
        internal AuthorizationResult()
        {
            expression = null;
            result = PolicyResult.NotHandled;
            succeededPolicies = new SortedSet<string>();
            failedPolicies = new SortedSet<string>();
            notHandledPolicies = new SortedSet<string>();
            policyResults = new Dictionary<string, PolicyResult>();
        }

        internal void CombineAsAnd(IAuthorizationResult result)
        {
            if (!result.IsAuthorizationRequired)
                return;

            if (!IsAuthorizationRequired)
            {
                expression = (IPolicyExpressionRootCombine)result.Expression;
                this.result = expression.Result;
                foreach (string key in result.SucceededPolicies)
                {
                    policyResults.Add(key, PolicyResult.Success);
                    succeededPolicies.Add(key);
                }
                foreach (string key in result.FailedPolicies)
                {
                    policyResults.Add(key, PolicyResult.Failed);
                    failedPolicies.Add(key);
                }
                foreach (string key in result.NotHandledPolicies)
                {
                    policyResults.Add(key, PolicyResult.NotHandled);
                    notHandledPolicies.Add(key);
                }
                return;
            }

            foreach (string key in result.SucceededPolicies)
            {
                if (policyResults.TryGetValue(key, out PolicyResult policyResult))
                {
                    switch (policyResult)
                    {
                        case PolicyResult.Success:
                        case PolicyResult.Failed:
                            // keep
                            break;
                        case PolicyResult.NotHandled:
                        default:
                            // overwrite
                            policyResults[key] = PolicyResult.Success;
                            succeededPolicies.Add(key);
                            notHandledPolicies.Remove(key);
                            break;
                    }
                }
                else
                {
                    policyResults.Add(key, PolicyResult.Success);
                    succeededPolicies.Add(key);
                }
            }
            foreach (string key in result.FailedPolicies)
            {
                if (policyResults.TryGetValue(key, out PolicyResult policyResult))
                {
                    switch (policyResult)
                    {
                        case PolicyResult.Success:
                        case PolicyResult.Failed:
                            // keep
                            break;
                        case PolicyResult.NotHandled:
                        default:
                            // overwrite
                            policyResults[key] = PolicyResult.Failed;
                            failedPolicies.Add(key);
                            notHandledPolicies.Remove(key);
                            break;
                    }
                }
                else
                {
                    policyResults.Add(key, PolicyResult.Failed);
                    failedPolicies.Add(key);
                }
            }
            foreach (string key in result.NotHandledPolicies)
            {
                if (!policyResults.ContainsKey(key))
                {
                    policyResults.Add(key, PolicyResult.NotHandled);
                    notHandledPolicies.Add(key);
                }
            }
            expression.Combine(result.Expression, PolicyOperator.And);
            this.result = expression.Result;
        }

        public static IAuthorizationResult Empty { get; } = new AuthorizationResult();

    }
}
