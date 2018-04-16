namespace AuthorizationCore.Expressions
{
    internal interface IPolicyExpressionRootCombine : IPolicyExpressionRoot
    {
        void Combine(IPolicyExpressionRoot right, PolicyOperator @operator);
    }
}
