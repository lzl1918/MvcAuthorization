using System.Text;

namespace AuthorizationCore.Expressions
{
    public interface IPolicyExpression
    {
        PolicyResult Result { get; }
    }

    public interface IPolicyOnlyExpression : IPolicyExpression
    {
        string Policy { get; }
    }

    public interface IPolicyExpressionWithOperator : IPolicyExpression
    {
        PolicyOperator Operator { get; }
        IPolicyExpression Left { get; }
        IPolicyExpression Right { get; }
    }
}
