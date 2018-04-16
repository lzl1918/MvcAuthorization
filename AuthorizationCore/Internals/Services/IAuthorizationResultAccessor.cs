namespace AuthorizationCore.Internals.Services
{
    internal interface IAuthorizationResultAccessor
    {
        IAuthorizationResult Result { get; set; }
    }

    internal sealed class AuthorizationResultAccessor : IAuthorizationResultAccessor
    {
        public IAuthorizationResult Result { get; set; }
    }
}
