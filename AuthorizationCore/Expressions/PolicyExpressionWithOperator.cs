namespace AuthorizationCore.Expressions
{
    internal sealed class PolicyExpressionWithOperator : IPolicyExpressionWithOperator
    {
        private static readonly PolicyResult[,] AndOperation = new PolicyResult[3, 3]
        {   //      success                   failed                notHadled
            { PolicyResult.Success,    PolicyResult.Failed, PolicyResult.NotHandled}, // success
            { PolicyResult.Failed,     PolicyResult.Failed, PolicyResult.Failed} ,    // failed
            { PolicyResult.NotHandled, PolicyResult.Failed, PolicyResult.NotHandled}, // notHandled
        };
        private static readonly PolicyResult[,] OrOperation = new PolicyResult[3, 3]
        {   //      success               failed                notHadled
            { PolicyResult.Success, PolicyResult.Success, PolicyResult.Success},    // success
            { PolicyResult.Success, PolicyResult.Failed,  PolicyResult.Failed} ,    // failed
            { PolicyResult.Success, PolicyResult.Failed,  PolicyResult.NotHandled}, // notHandled
        };

        private readonly PolicyOperator @operator;
        private readonly IPolicyExpression left;
        private readonly IPolicyExpression right;
        private readonly PolicyResult result;
        private int? hashCode;

        public PolicyOperator Operator => @operator;
        public IPolicyExpression Left => left;
        public IPolicyExpression Right => right;
        public PolicyResult Result => result;

        public PolicyExpressionWithOperator(PolicyOperator @operator, IPolicyExpression left, IPolicyExpression right)
        {
            this.@operator = @operator;
            this.left = left;
            this.right = right;
            PolicyResult leftResult = left.Result;
            PolicyResult rightResult = right.Result;
            switch (@operator)
            {
                case PolicyOperator.And:
                    this.result = AndOperation[(int)leftResult, (int)rightResult];
                    break;
                case PolicyOperator.Or:
                default:
                    this.result = OrOperation[(int)leftResult, (int)rightResult];
                    break;
            }

        }

        public override string ToString()
        {
            switch (@operator)
            {
                case PolicyOperator.And:
                    return $"{left} & {right}";
                case PolicyOperator.Or:
                default:
                    return $"{left} | {right}";
            }

        }
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj.GetType() != typeof(PolicyExpressionWithOperator))
                return false;
            PolicyExpressionWithOperator exp = (PolicyExpressionWithOperator)obj;
            return exp.left.Equals(left) && exp.right.Equals(right) && exp.@operator == @operator;
        }
        public override int GetHashCode()
        {
            if (hashCode.HasValue)
                return hashCode.Value;
            hashCode = left.GetHashCode() ^ right.GetHashCode() ^ result.GetHashCode();
            return hashCode.Value;
        }
    }
}
