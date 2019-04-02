using System;
using System.Threading.Tasks;
using IdentityManager2.Assets;
using IdentityManager2.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        [Authorize(IdentityManagerConstants.IdMgrAuthPolicy)]
        [Route("", Name = IdentityManagerConstants.RouteNames.Home)]
        public IActionResult Index()
        {
            return new EmbeddedHtmlResult(
                Request.PathBase, 
                "IdentityManager2.Assets.Templates.index.html",
                config.SecurityConfiguration);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("api/login", Name = IdentityManagerConstants.RouteNames.Login)]
        public async Task<IActionResult> Login(string returnUrl)
        {
            var result = await HttpContext.AuthenticateAsync(config.SecurityConfiguration.HostAuthenticationType);
            if (result.Succeeded)
            {
                await HttpContext.SignInAsync(IdentityManagerConstants.LocalApiScheme, result.Principal);
                return Redirect(returnUrl);
            }

            return Challenge(config.SecurityConfiguration.HostChallengeType);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("logout", Name = IdentityManagerConstants.RouteNames.Logout)]
        public async Task<IActionResult> Logout()
        {
            await config.SecurityConfiguration.SignOut(HttpContext);
            return RedirectToRoute(IdentityManagerConstants.RouteNames.Home, null);
        }
    }
}