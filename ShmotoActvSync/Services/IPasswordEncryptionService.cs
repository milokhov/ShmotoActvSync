namespace ShmotoActvSync.Services
{
    public interface IPasswordEncryptionService
    {
        string DecryptPassword(string password);
        string EncryptPassword(string password);
    }
}