using AuthorizationCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthorizationTest
{
    public class TreeGreaterThanPolicy : IPolicy<User, Tree>
    {

    }
    public class TreeGreaterThanPolicyHandler : IPolicyHandler<User, Tree, TreeGreaterThanPolicy>
    {
        public AuthorizationResult OnAuthorization(User user, Tree target, TreeGreaterThanPolicy policy)
        {
            if (user.Age > target.Age)
                return AuthorizationResult.Success;
            else
                return AuthorizationResult.Failed;
        }
    }
}
