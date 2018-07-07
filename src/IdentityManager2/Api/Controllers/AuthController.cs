using System;
using System.Threading.Tasks;
using IdentityManager2.Configuration;
using IdentityManager2.Configuration.Hosting;
using IdentityManager2.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace IdentityManager2.Api.Controllers
{
    public class AuthController : Controller
    {
        private readonly ITokenProvider<AuthenticationTicket> tokenProvider;
        private readonly IdentityManagerOptions options;

        public AuthController(ITokenProvider<AuthenticationTicket> tokenProvider, IdentityManagerOptions options)
        {
            this.tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        [HttpGet, Route("authorize")]
        public async Task<IActionResult> Authorize(OAuthModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            if (model.ClientId != Constants.IdMgrClientId) return StatusCode(401);
            if (model.ResponseType != "token") return StatusCode(401);
            
            var result = await HttpContext.AuthenticateAsync(options.SecurityConfiguration.HostAuthenticationType);
            if (!result.Succeeded) return Challenge(options.SecurityConfiguration.HostChallengeType);

            var token = tokenProvider.Generate(result.Ticket);
            var expiresIn = (long)options.SecurityConfiguration.TokenExpiration.TotalSeconds;
            
            var returnUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}{Constants.CallbackFragment}&access_token={token}&token_type=Bearer&expires_in={expiresIn}&state={model.State}";

            return Redirect(returnUrl);
        }
    }

    public class OAuthModel
    {
        [FromQuery(Name = "client_id")]
        public string ClientId { get; set; }

        [FromQuery(Name = "response_type")]
        public string ResponseType { get; set; }

        [FromQuery]
        public string State { get; set; }
    }
}