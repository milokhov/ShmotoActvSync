using System;
using ShmotoActvSync.Models;

namespace ShmotoActvSync.Services
{
    public interface IDbService
    {
        void AddOrUpdateUser(User user);
        User FindUserByStravaId(long stravaId);
        void StoreMotoActvCredentials(string username, string password);
        void ResetMotoActvCredentials();
        (string username, string password) RetrieveMotoActvCredentials();
        User GetCurrentUser();
        User GetLeastRecentSyncedUser();
        void UpdateSyncStatus(User user, Exception e);
        void UpdateSyncStatus(User user, bool MorePending);
    }
}