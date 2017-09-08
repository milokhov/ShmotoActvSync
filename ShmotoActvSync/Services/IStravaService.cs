using System.IO;
using System.Threading.Tasks;

namespace ShmotoActvSync.Services
{
    public interface IStravaService
    {

        Task<UploadActivityResult> UploadActivity(Stream stream, string fileName, string activityId);
    }
}