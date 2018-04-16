using System.Collections.Generic;

namespace AuthorizationCore.Expressions
{
    public interface IPolicyExpressionRoot : IEnumerable<IPolicyOnlyExpression>
    {
        PolicyResult Result { get; }
        IPolicyExpression Expression { get; }
        bool IsPolicyOnly { get; }
        string PolicyName { get; }
    }
}
