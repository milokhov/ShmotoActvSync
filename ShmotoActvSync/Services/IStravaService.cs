using System.IO;
using System.Threading.Tasks;

namespace ShmotoActvSync.Services
{
    public interface IStravaService
    {

        Task UploadActivity(Stream stream, string fileName, string activityId);
    }
}