using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShmotoActvSync.Services
{
    public class PasswordEncryptionService : IPasswordEncryptionService
    {
        public string EncryptPassword(string password)
        {
            return password;
        }

        public string DecryptPassword(string password)
        {
            return password;
        }
    }
}
