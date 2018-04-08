using System.Collections.Generic;
using System.Text;

namespace AuthorizationCore.Services
{

    public interface IAuthorizationService<TUser>
    {
        AuthorizationResult TryAuthorize(string policy);
        IAuthorizationExpressionBuilder CreateExpressionBuilder();
    }

    public interface IAuthorizationExpressionBuilder
    {
        IAuthorizationExpressionBuilder And(string policy);
        IAuthorizationExpressionBuilder Or(string policy);
        IAuthorizationExpressionBuilder AndNot(string policy);
        IAuthorizationExpressionBuilder OrNot(string policy);
        IAuthorizationExpressionBuilder Not(string policy);

        IAuthorizationExpression Build();
    }

    public interface IAuthorizationExpression
    {

    }

}
