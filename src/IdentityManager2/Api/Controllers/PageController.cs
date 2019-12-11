using System;
using System.Text.Json;
using System.Threading.Tasks;
using IdentityManager2.Api.Models;
using IdentityManager2.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace IdentityManager2.Api.Controllers
{
    [SecurityHeaders]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class PageController : Controller
    {
        private readonly IdentityManagerOptions config;
        public PageController(IOptions<IdentityManagerOptions> config)
        {
            this.config = config?.Value ?? throw new ArgumentNullException(nameof(config));
        }

        [HttpGet]
        [Route("", Name = IdentityManagerConstants.RouteNames.Home)]
        public async Task<IActionResult> Index()
        {
            var authResult = await HttpContext.AuthenticateAsync(config.SecurityConfiguration.HostAuthenticationType);

            return View("/Areas/IdentityManager/Pages/Index.cshtml", new PageModel
            {
                PathBase = Request.PathBase,
                Model = JsonSerializer.Serialize(new PageModelParams
                {
                    PathBase = Request.PathBase,
                    ShowLoginButton = !authResult.Succeeded
                })
            });
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("api/login", Name = IdentityManagerConstants.RouteNames.Login)]
        public async Task<IActionResult> Login()
        {
            var authResult = await HttpContext.AuthenticateAsync(config.SecurityConfiguration.HostAuthenticationType);
            if (authResult.Succeeded)
            {
                await HttpContext.SignInAsync(IdentityManagerConstants.LocalApiScheme, authResult.Principal);
                return RedirectToAction("Index");
            }

            return Challenge(new AuthenticationProperties {RedirectUri = Url.Action("Login")}, config.SecurityConfiguration.HostChallengeType);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("api/login/refresh")]
        public async Task<IActionResult> Refresh()
        {
            var authResult = await HttpContext.AuthenticateAsync(config.SecurityConfiguration.HostAuthenticationType);
            if (authResult.Succeeded)
            {
                await HttpContext.SignInAsync(IdentityManagerConstants.LocalApiScheme, authResult.Principal);
                return Ok();
            }

            return Unauthorized();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("api/logout", Name = IdentityManagerConstants.RouteNames.Logout)]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityManagerConstants.LocalApiScheme);

            await config.SecurityConfiguration.SignOut(HttpContext);

            return RedirectToRoute(IdentityManagerConstants.RouteNames.Home, null);
        }
    }
}