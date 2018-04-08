namespace AuthorizationCore
{
    public interface IPolicyHandler<TUser, TPolicy> where TPolicy : IPolicy<TUser>
    {
        AuthorizationResult OnAuthorization(TUser user, TPolicy policy);
    }

    public interface IPolicyHandler<TUser, TObject, TPolicy> where TPolicy : IPolicy<TUser, TObject>
    {
        AuthorizationResult OnAuthorization(TUser user, TObject target, TPolicy policy);
    }
}
