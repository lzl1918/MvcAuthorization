using AuthorizationCore;

namespace AuthorizationTest
{
    public class AgeGreaterThanPolicy : IPolicy<User>
    {
        public int MinAge { get; }

        public AgeGreaterThanPolicy(int minAge)
        {
            MinAge = minAge;
        }
    }


    public class AgeGreaterThanPolicyHandler : IPolicyHandler<User, AgeGreaterThanPolicy>
    {
        public PolicyResult OnAuthorization(User user, AgeGreaterThanPolicy policy)
        {
            if (user.Age > policy.MinAge)
                return PolicyResult.Success;
            else
                return PolicyResult.Failed;
        }
    }

}
