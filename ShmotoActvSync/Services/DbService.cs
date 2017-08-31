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
        private IPasswordEncryptionService passwordEncryptionService;
        private ICurrentUserService currentUserService;

        public DbService(IPasswordEncryptionService passwordEncryptionService, ICurrentUserService currentUserService)
        {
            this.passwordEncryptionService = passwordEncryptionService;
            this.currentUserService = currentUserService;
        }

        public void AddOrUpdateUser(User user)
        {
            using (var db = new LiteRepository(connectionString))
            {
                var dbUser = db.SingleOrDefault<User>(it => it.StravaId == user.StravaId);
                if (dbUser == null)
                {
                    db.Insert(user);
                } else
                {
                    db.Update(user);
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

        public User GetCurrentUser()
        {
            using (var db = new LiteRepository(connectionString))
            {
                return db.Single<User>(it => it.StravaId == currentUserService.GetCurrentUser().StravaID);
            }
        }

        public void ResetMotoActvCredentials()
        {
            using (var db = new LiteRepository(connectionString))
            {
                var user = db.Single<User>(it => it.StravaId == currentUserService.GetCurrentUser().StravaID);
                user.MotoUserName = null;
                user.MotoPassword = null;
                db.Update(user);
            }
        }

        public (string username, string password) RetrieveMotoActvCredentials()
        {
            using (var db = new LiteRepository(connectionString))
            {
                var user = db.Single<User>(it => it.StravaId == currentUserService.GetCurrentUser().StravaID);
                return (user.MotoUserName, passwordEncryptionService.DecryptPassword(user.MotoPassword));
            }
        }

        public void StoreMotoActvCredentials(string username, string password)
        {
            using (var db = new LiteRepository(connectionString))
            {
                var user = db.Single<User>(it => it.StravaId == currentUserService.GetCurrentUser().StravaID);
                user.MotoUserName = username;
                user.MotoPassword = passwordEncryptionService.EncryptPassword(password);
                db.Update(user);
            }
        }
    }
}
