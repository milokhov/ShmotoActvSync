using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShmotoActvSync.Models
{
    public class User
    {
        public int Id { get; set; }
        public string StravaUserName { get; set; }
        public long StravaId { get; set; }
        public string StravaToken { get; set; }
        public string MotoUserName { get; set; }
        public string MotoPassword { get; set; }
        public DateTime? LastSyncedDate { get; set; }
        public string LastSyncStatus { get; set; }
        public string[] SyncedActivities { get; set; }
    }
}
