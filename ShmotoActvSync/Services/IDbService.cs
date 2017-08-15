using ShmotoActvSync.Models;

namespace ShmotoActvSync.Services
{
    public interface IDbService
    {
        void AddUser(User user);
        User FindUserByStravaId(long stravaId);
    }
}