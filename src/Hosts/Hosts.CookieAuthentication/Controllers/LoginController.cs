using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Hosts.Shared.InMemory;
using IdentityManager2;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Hosts.CookieAuthentication
{
    public class LoginController : Controller
    {
        private readonly ICollection<InMemoryUser> users;

        public LoginController(ICollection<InMemoryUser> users)
        {
            this.users = users ?? throw new ArgumentNullException(nameof(users));
        }

        [HttpGet("login")]
        public IActionResult Login(string returnUrl)
        {
            return View(new LoginModel {ReturnUrl = returnUrl});
        }

        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            var user = users.FirstOrDefault(x => x.Username == model.Username && x.Password == model.Password);

            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim("sub", user.Subject),
                    new Claim("name", user.Username)
                };

                foreach (var role in user.Claims.Where(x => x.Type == IdentityManagerConstants.ClaimTypes.Role))
                {
                    claims.Add(new Claim(IdentityManagerConstants.ClaimTypes.Role, role.Value));
                }

                await HttpContext.SignInAsync("cookie", new ClaimsPrincipal(new ClaimsIdentity(claims, "cookie")));

                if (Url.IsLocalUrl(model.ReturnUrl)) return LocalRedirect(model.ReturnUrl);
                return LocalRedirect("~/");
            }

            ModelState.AddModelError("", "Invalid username or password");
            return View(model);
        }
    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string ReturnUrl { get; set; }
    }
}