namespace ShmotoActvSync.Services
{
    public interface IMotoActvCredentialsProvider
    {
        (string username, string password) GetCredentials();
    }
}