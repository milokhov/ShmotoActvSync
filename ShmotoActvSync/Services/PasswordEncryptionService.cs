using Microsoft.AspNetCore.DataProtection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShmotoActvSync.Services
{
    public class PasswordEncryptionService : IPasswordEncryptionService
    {
        private IDataProtector protector;

        public PasswordEncryptionService(IDataProtectionProvider provider)
        {
            protector = provider.CreateProtector("MotoPassword");
        }

        public string EncryptPassword(string password)
        {
            return protector.Protect(password);
        }

        public string DecryptPassword(string password)
        {
            return protector.Unprotect(password);
        }
    }
}
