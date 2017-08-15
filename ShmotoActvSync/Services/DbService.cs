using LiteDB;
using ShmotoActvSync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShmotoActvSync.Services
{

    public class DbService : IDbService
    {
        private const string connectionString = "litedb.db";

        public void AddUser(User user)
        {
            using (var db = new LiteRepository(connectionString))
            {
                var dbUser = db.SingleOrDefault<User>(it => it.StravaId == user.StravaId);
                if (dbUser == null)
                {
                    db.Insert(user);
                }

            }
        }

        public User FindUserByStravaId(long stravaId)
        {
            using (var db = new LiteRepository(connectionString))
            {
                return db.SingleOrDefault<User>(it => it.StravaId == stravaId);
            }
        }
    }
}
