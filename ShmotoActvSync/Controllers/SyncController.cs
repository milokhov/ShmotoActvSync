using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShmotoActvSync.Services;
using Microsoft.AspNetCore.Authorization;

namespace ShmotoActvSync.Controllers
{
    public class SyncController : Controller
    {
        private readonly ISyncerService syncerService;

        public SyncController(ISyncerService syncerService)
        {
            this.syncerService = syncerService;
        }

        [Route("sync/sync")]
        [HttpPost]
        public async Task<ActionResult> Sync()
        {
            await syncerService.Sync();
            return new JsonResult(new { Success = "true" });
        }
    }
}