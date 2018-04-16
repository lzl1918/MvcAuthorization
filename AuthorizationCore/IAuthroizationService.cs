using System.Text;

namespace AuthorizationCore
{

    public interface IAuthorizationService<TUser>
    {
        PolicyResult TryAuthorize(string policy);
    }

    public static class AuthorizationServiceExtensions
    {
        public static bool TryAuthorize<TUser>(this IAuthorizationService<TUser> service, string policy, bool failedIfNotHandled = true)
        {
            PolicyResult result = service.TryAuthorize(policy);
            switch (result)
            {
                case PolicyResult.Success:
                    return true;
                case PolicyResult.Failed:
                    return false;
                case PolicyResult.NotHandled:
                default:
                    return !failedIfNotHandled;
            }
        }
    }
}
