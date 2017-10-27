using System.Threading.Tasks;

namespace ShmotoActvSync.Services
{
    public interface ISyncerService
    {
        Task<SyncResult> Sync();
    }
}