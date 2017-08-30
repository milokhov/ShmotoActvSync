using System.IO;
using System.Threading.Tasks;

namespace ShmotoActvSync.Services
{
    public interface IMotoActvService
    {
        Task<(bool, string)> VerifyPassword(string username, string password);
        Task<MotoActvWorkouts> GetRecentWorkouts();
        Task<Stream> RetrieveWorkout(string workoutId);
    }
}