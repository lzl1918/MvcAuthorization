using AuthorizationCore.Attributes;

namespace AuthorizationTest
{
    public class UserAuthorizationRequiredAttribute : AuthorizationRequiredAttribute
    {
        public UserAuthorizationRequiredAttribute(string expression, AuthorizationFailedAction failedAction = AuthorizationFailedAction.Return401, bool failedIfNotHandled = true) : base(expression, typeof(User), failedAction, failedIfNotHandled)
        {
        }
    }
}
