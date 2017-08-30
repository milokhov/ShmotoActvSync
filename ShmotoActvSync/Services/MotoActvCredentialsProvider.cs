using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShmotoActvSync.Services
{
    public class MotoActvCredentialsProvider : IMotoActvCredentialsProvider
    {
        private readonly IDbService dbService;

        public MotoActvCredentialsProvider(IDbService dbService)
        {
            this.dbService = dbService;
        }

        public (string username, string password) GetCredentials()
        {
            return dbService.RetrieveMotoActvCredentials();
        }
    }
}
