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
        private TimeSpan syncTimespan = TimeSpan.FromHours(1);

        public SyncerService(IDbService dbService, IMotoActvService motoActvService, IStravaService stravaService, ICurrentUserService currentUserService)
        {
            this.dbService = dbService;
            this.motoActvService = motoActvService;
            this.stravaService = stravaService;
            this.currentUserService = currentUserService;
        }

        public async Task<SyncResult> Sync()
        {
            var user = dbService.GetLeastRecentSyncedUser();
            // Don't sync if last one was less than 4 hours ago
            if (!user.AdditionalSyncPending && (user.LastSyncedDate.HasValue && DateTime.Now - user.LastSyncedDate.Value < TimeSpan.FromHours(4))) return null;
            if (user.AdditionalSyncPending && (user.LastSyncedDate.HasValue && DateTime.Now - user.LastSyncedDate.Value < TimeSpan.FromMinutes(5))) return null;

            currentUserService.OverrideCurrentUser(new CurrentUserInfo
            {
                StravaID = user.StravaId,
                StravaLogin = user.StravaUserName
            });

            try
            {
                await UpdateSyncedActivities(user);
                var syncedActivity = await SyncActivityForUser(user);
                if (syncedActivity.Workout!=null) AddSyncedActivity(user, syncedActivity.Workout.WorkoutActivityId);
                dbService.UpdateSyncStatus(user, syncedActivity.More);
                return new SyncResult { UserName = user.StravaUserName, WorkoutId = syncedActivity.Workout?.WorkoutActivityId, ActivityDate = syncedActivity.Workout?.StartTime };
            }
            catch (Exception e)
            {
                dbService.UpdateSyncStatus(user, e);
                throw;
            }
        }

        private void AddSyncedActivity(User user, string syncedActivityId)
        {
            user.SyncedActivities = user.SyncedActivities.Union(new[] { syncedActivityId }).ToArray();
            dbService.AddOrUpdateUser(user);
        }

        private async Task UpdateSyncedActivities(User user)
        {
            var activities = await stravaService.GetAthleteActivities();
            user.SyncedActivities = user.SyncedActivities.Union(activities.Where(it => it.ExternalId != null && it.ExternalId.EndsWith(".tcx"))
                .Select(it => it.ExternalId.Substring(0, it.ExternalId.Length - 4))
                ).Distinct().ToArray();
            dbService.AddOrUpdateUser(user);
        }

        private async Task<(MotoActvWorkout Workout, bool More)> SyncActivityForUser(User user)
        {
            var recentWorkouts = await motoActvService.GetWorkouts(DateTime.Now.AddDays(-90), DateTime.Now); // activities from last 90 days
            var workoutsToSync = recentWorkouts.Workouts
                    .OrderBy(it => it.StartTime)
                    .Where(it => !user.SyncedActivities.Contains(it.WorkoutActivityId));

            var workoutToSync = workoutsToSync.FirstOrDefault();
            if (workoutToSync==null) return (null,false);
            var workoutStream = await motoActvService.RetrieveWorkout(workoutToSync.WorkoutActivityId);
            var uploadResult = await stravaService.UploadActivity(workoutStream, "activity.tcx", workoutToSync.WorkoutActivityId);
            if (!uploadResult.Success)
            {
                if (!uploadResult.Error.Contains("duplicate"))
                {
                    throw new Exception($"Error while uploading:{uploadResult.Error}");
                }
            }
            return (workoutToSync, workoutsToSync.Count() > 1);
        }
    }

    public class SyncResult
    {
        public string UserName { get; set; }
        public string WorkoutId { get; set; }
        public DateTime? ActivityDate { get; set; }
    }
}
