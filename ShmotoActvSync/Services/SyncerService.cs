using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShmotoActvSync.Models;

namespace ShmotoActvSync.Services
{
    public class SyncerService : ISyncerService
    {
        private readonly IDbService dbService;
        private readonly IMotoActvService motoActvService;
        private readonly IStravaService stravaService;
        private readonly ICurrentUserService currentUserService;

        public SyncerService(IDbService dbService, IMotoActvService motoActvService, IStravaService stravaService, ICurrentUserService currentUserService)
        {
            this.dbService = dbService;
            this.motoActvService = motoActvService;
            this.stravaService = stravaService;
            this.currentUserService = currentUserService;
        }

        public async Task Sync()
        {
            var user = dbService.GetLeastRecentSyncedUser();
            // Don't sync if last one was less than 4 hours ago
            if (user.LastSyncedDate.HasValue && DateTime.Now - user.LastSyncedDate.Value < TimeSpan.FromHours(4)) return;

            currentUserService.OverrideCurrentUser(new CurrentUserInfo
            {
                StravaID = user.StravaId,
                StravaLogin = user.StravaUserName
            });

            try
            {
                await UpdateSyncedActivities(user);
                var syncedActivityId = await SyncActivityForUser(user);

                //dbService.UpdateSyncStatus(user);
            }
            catch (Exception e)
            {
                // TODO
                dbService.UpdateSyncStatus(user, e);
                // Log
            }
        }

        private async Task UpdateSyncedActivities(User user)
        {
            var activities = await stravaService.GetAthleteActivities();
            user.SyncedActivities = activities.Where(it => it.ExternalId != null && it.ExternalId.EndsWith(".tcx"))
                .Select(it => it.ExternalId.Substring(0, it.ExternalId.Length - 4))
                .ToArray();
            dbService.AddOrUpdateUser(user);
        }

        private async Task<string> SyncActivityForUser(User user)
        {
            var recentWorkouts = await motoActvService.GetWorkouts(DateTime.Now.AddDays(-30), DateTime.Now); // activities from last 30 days
            var workoutIdToSync = recentWorkouts.Workouts
                    .OrderBy(it => it.StartTime)
                    .Where(it => !user.SyncedActivities.Contains(it.WorkoutActivityId))
                    .Select(it => it.WorkoutActivityId)
                    .FirstOrDefault();
            if (string.IsNullOrEmpty(workoutIdToSync)) return null;
            var workoutStream = await motoActvService.RetrieveWorkout(workoutIdToSync);
            //await stravaService.UploadActivity(workoutStream, "activity.tcx", workoutIdToSync);
            return workoutIdToSync;
        }
    }
}
