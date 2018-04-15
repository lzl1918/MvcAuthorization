using System.Text;

namespace AuthorizationCore.Services
{

    public interface IAuthorizationService<TUser>
    {
        PolicyResult TryAuthorize(string policy);
    }
}
