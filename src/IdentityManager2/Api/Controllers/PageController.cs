using System;
using System.Threading.Tasks;
using IdentityManager2.Api.Models;
using IdentityManager2.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IdentityManager2.Api.Controllers
{
    [SecurityHeaders]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class PageController : Controller
    {
        private readonly IdentityManagerOptions config;
        public PageController(IdentityManagerOptions config)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
        }

        [HttpGet]
        [Route("", Name = IdentityManagerConstants.RouteNames.Home)]
        public async Task<IActionResult> Index()
        {
            var authResult = await HttpContext.AuthenticateAsync(config.SecurityConfiguration.HostAuthenticationType);

            return View("/Areas/IdentityManager/Pages/Index.cshtml", new PageModel
            {
                PathBase = Request.PathBase,
                Model = JsonConvert.SerializeObject(new
                {
                    PathBase = Request.PathBase,
                    ShowLoginButton = !authResult.Succeeded
                })
            });
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("api/login", Name = IdentityManagerConstants.RouteNames.Login)]
        public async Task<IActionResult> Login(string returnUrl)
        {
            var authResult = await HttpContext.AuthenticateAsync(config.SecurityConfiguration.HostAuthenticationType);
            if (authResult.Succeeded)
            {
                await HttpContext.SignInAsync(IdentityManagerConstants.LocalApiScheme, authResult.Principal);
                return Redirect(returnUrl);
            }

            return Challenge(config.SecurityConfiguration.HostChallengeType);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("logout", Name = IdentityManagerConstants.RouteNames.Logout)]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityManagerConstants.LocalApiScheme);

            await config.SecurityConfiguration.SignOut(HttpContext);

            return RedirectToRoute(IdentityManagerConstants.RouteNames.Home, null);
        }
    }
}