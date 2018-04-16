using System;
using System.Collections;
using System.Collections.Generic;

namespace AuthorizationCore.Expressions
{
    public class PolicyExpressionPolicyEnumerator : IEnumerator<IPolicyOnlyExpression>
    {
        private IPolicyOnlyExpression policyOnly;
        private IPolicyExpressionRoot root;
        private Stack<IPolicyExpression> stack;
        private IPolicyOnlyExpression current;

        public IPolicyOnlyExpression Current => current;
        object IEnumerator.Current => current;

        internal PolicyExpressionPolicyEnumerator(IPolicyExpressionRoot root)
        {
            this.root = root;
            if (root.IsPolicyOnly)
            {
                policyOnly = (IPolicyOnlyExpression)root.Expression;
            }
            else
            {
                stack = new Stack<IPolicyExpression>();
                var exp = (IPolicyExpressionWithOperator)root.Expression;
                stack.Push(exp.Right);
                stack.Push(exp.Left);
            }
        }
        public void Dispose()
        {
            policyOnly = null;
            current = null;
            root = null;
            if (stack != null)
                stack.Clear();
            stack = null;
        }

        public bool MoveNext()
        {
            if (policyOnly != null)
            {
                if (current == policyOnly)
                    return false;
                current = policyOnly;
                return true;
            }

            if (stack.Count <= 0)
                return false;

            IPolicyExpression top = stack.Pop();
            while (true)
            {
                switch (top)
                {
                    case IPolicyExpressionWithOperator op:
                        stack.Push(op.Right);
                        top = op.Left;
                        break;
                    case IPolicyOnlyExpression policyOnly:
                        current = policyOnly;
                        return true;
                    default:
                        throw new Exception($"unsupported expression node of type {top.GetType()}");
                }
            }
        }

        public void Reset()
        {
            if (policyOnly != null)
                current = null;
            else
            {
                stack.Clear();
                var exp = (IPolicyExpressionWithOperator)root.Expression;
                stack.Push(exp.Right);
                stack.Push(exp.Left);
            }
        }
    }
}
