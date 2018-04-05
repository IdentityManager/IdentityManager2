using System;
using System.Threading.Tasks;
using IdentityManager2.Assets;
using IdentityManager2.Configuration;
using IdentityManager2.Core;
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
        [AllowAnonymous]
        [Route("", Name = Constants.RouteNames.Home)]
        public IActionResult Index()
        {
            return new EmbeddedHtmlResult(
                Request.PathBase, 
                "IdentityManager2.Assets.Templates.index.html",
                config.SecurityConfiguration);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("logout", Name = Constants.RouteNames.Logout)]
        public async Task<IActionResult> Logout()
        {
            await config.SecurityConfiguration.SignOut(HttpContext);
            return RedirectToRoute(Constants.RouteNames.Home, null);
        }
    }
}