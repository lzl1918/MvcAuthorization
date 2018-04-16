namespace AuthorizationCore.Expressions
{
    internal sealed class PolicyOnlyExpression : IPolicyOnlyExpression
    {
        private readonly string policy;
        private readonly PolicyResult result;

        public string Policy => policy;
        public PolicyResult Result => result;

        public PolicyOnlyExpression(string policy, PolicyResult result)
        {
            this.policy = policy;
            this.result = result;
        }

        public override string ToString()
        {
            return $"{policy}: {result}";
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj.GetType() != typeof(PolicyOnlyExpression))
                return false;
            PolicyOnlyExpression exp = (PolicyOnlyExpression)obj;
            return exp.result == this.result && exp.policy == this.policy;
        }
        public override int GetHashCode()
        {
            return policy.GetHashCode() ^ result.GetHashCode();
        }
    }
}
