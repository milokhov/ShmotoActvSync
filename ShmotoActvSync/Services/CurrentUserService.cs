using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ShmotoActvSync.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private IHttpContextAccessor httpContext;

        public CurrentUserService(IHttpContextAccessor httpContext)
        {
            this.httpContext = httpContext;
        }

        public CurrentUserInfo GetCurrentUser()
        {
            if (!httpContext.HttpContext.User.HasClaim(it => it.Type == "strava_id"))
            {
                throw new InvalidAuthenticationException("Required claim not found");
            }
            return new CurrentUserInfo
            {
                StravaID = long.Parse(httpContext.HttpContext.User.FindFirst("strava_id").Value),
                StravaLogin = httpContext.HttpContext.User.FindFirst(ClaimTypes.Name).Value
            };
        }
    }

    public class CurrentUserInfo
    {
        public long StravaID { get; set; }
        public string StravaLogin { get; set; }
    }
}
