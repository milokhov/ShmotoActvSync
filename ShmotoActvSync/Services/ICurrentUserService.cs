namespace ShmotoActvSync.Services
{
    public interface ICurrentUserService
    {
        CurrentUserInfo GetCurrentUser();
        void OverrideCurrentUser(CurrentUserInfo currentUserInfo);
    }
}