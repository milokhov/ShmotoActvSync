using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Authentication;

namespace ShmotoActvSync.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult LoginPostStrava()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = "/Account/ExternalLoginCallback",
            };
            return Challenge(properties, "Strava");
        }

        public IActionResult ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
                return RedirectToAction("LoginError", "Account");

            if (returnUrl != null)
                return new LocalRedirectResult(returnUrl);
            else
                return RedirectToAction("Index", "Home");
        }

    }
}