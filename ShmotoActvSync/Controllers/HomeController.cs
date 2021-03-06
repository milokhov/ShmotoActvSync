﻿using System;
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
        private IMotoActvService motoActvService;
        private readonly IStravaService stravaService;

        public HomeController(IDbService dbService, IMotoActvService motoActvService, IStravaService stravaService, ISyncerService syncerService)
        {
            this.dbService = dbService;
            this.motoActvService = motoActvService;
            this.stravaService = stravaService;
        }

        public IActionResult Index()
        {
            var user = dbService.FindUserByStravaId(long.Parse(HttpContext.User.FindFirst("strava_id").Value));
            var viewModel = new UserViewModel
            {
                UserName = user.StravaUserName,
                AssociatedToMoto = !string.IsNullOrEmpty(user.MotoUserName),
                MotoUserName = user.MotoUserName
            };
            return View(viewModel);
        }

        [Route("link")]
        [HttpPost]
        public async Task<IActionResult> Link(string username, string password)
        {
            var (valid, error) = await motoActvService.VerifyPassword(username, password);
            if (!valid)
            {
                TempData["Error"] = error;
            }
            else
            {
                dbService.StoreMotoActvCredentials(username, password);
            }
            return RedirectToAction("Index");
        }

        [Route("unlink")]
        [HttpPost]
        public IActionResult UnLink()
        {
            dbService.ResetMotoActvCredentials();
            return RedirectToAction("Index");
        }

        [Route("sync")]
        [HttpPost]
        public async Task<IActionResult> SyncAsync()
        {
            //var workouts = await motoActvService.GetRecentWorkouts();
            //var workout = await motoActvService.RetrieveWorkout(workouts.Workouts[0].WorkoutActivityId);
            //workout.Seek(0, System.IO.SeekOrigin.Begin);
            //await stravaService.UploadActivity(workout, "activity.tcx", workouts.Workouts[0].WorkoutActivityId);

            // Find user with earliest activity sync
            // perform sync for him



            return RedirectToAction("Index");
        }
    }
}
