using System;
using System.Collections;
using System.Collections.Generic;

namespace AuthorizationCore.Expressions
{
    internal sealed class PolicyExpressionRoot : IPolicyExpressionRootCombine
    {
        private PolicyResult result;
        private IPolicyExpression expression;
        private bool isPolicyOnly;
        private string policyName;

        public PolicyResult Result => result;
        public IPolicyExpression Expression => expression;
        public bool IsPolicyOnly => isPolicyOnly;
        public string PolicyName => policyName;

        public PolicyExpressionRoot(IPolicyExpression expression)
        {
            this.expression = expression;
            result = expression.Result;

            switch (expression)
            {
                case IPolicyOnlyExpression policyOnly:
                    isPolicyOnly = true;
                    policyName = policyOnly.Policy;
                    break;
                case IPolicyExpressionWithOperator withOperator:
                    isPolicyOnly = false;
                    policyName = null;
                    break;
                default:
                    throw new Exception($"expression of type {expression.GetType()} is not supported");
            }

        }

        public void Combine(IPolicyExpressionRoot right, PolicyOperator @operator)
        {
            if (right == null)
                throw new ArgumentNullException(nameof(right));

            PolicyExpressionWithOperator exp = new PolicyExpressionWithOperator(@operator, expression, right.Expression);
            if (isPolicyOnly)
            {
                isPolicyOnly = false;
                policyName = null;
            }
            expression = exp;
            result = exp.Result;
        }

        public IEnumerator<IPolicyOnlyExpression> GetEnumerator()
        {
            return new PolicyExpressionPolicyEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new PolicyExpressionPolicyEnumerator(this);
        }
    }
}
