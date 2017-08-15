using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ShmotoActvSync.ViewModels;
using ShmotoActvSync.Services;

namespace ShmotoActvSync.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private IDbService dbService;

        public HomeController(IDbService dbService)
        {
            this.dbService = dbService;
        }

        public IActionResult Index()
        {
            var user = dbService.FindUserByStravaId(long.Parse(HttpContext.User.FindFirst("strava_id").Value));
            var viewModel = new UserViewModel
            {
                UserName = user.StravaUserName
            };
            return View(viewModel);
        }

        [Route("link")]
        [HttpPost]
        public IActionResult Link(string username, string password)
        {
            // TODO - link
            return RedirectToAction("Index");
        }
    }
}
