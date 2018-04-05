using System.Text;
using System.Threading.Tasks;
using IdentityManager2.Configuration;
using IdentityManager2.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IdentityManager2.Assets
{
    internal class EmbeddedHtmlResult : IActionResult
    {
        private readonly string path;
        private readonly string file;
        private readonly SecurityConfiguration securityConfiguration;
        private readonly string authorizationEndpoint;

        public EmbeddedHtmlResult(PathString pathBase, string file, SecurityConfiguration securityConfiguration)
        {
            path = pathBase.Value;
            this.file = file;
            this.securityConfiguration = securityConfiguration;
            authorizationEndpoint = pathBase + Constants.AuthorizePath;
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            var html = AssetManager.LoadResourceString(file,
                new
                {
                    pathBase = path,
                    model = JsonConvert.SerializeObject(new
                    {
                        PathBase = path,
                        ShowLoginButton = securityConfiguration.ShowLoginButton,
                        oauthSettings = new
                        {
                            authorization_endpoint = authorizationEndpoint,
                            client_id = Constants.IdMgrClientId
                        }
                    })
                });

            context.HttpContext.Response.ContentType = "text/html";
            await context.HttpContext.Response.WriteAsync(html, Encoding.UTF8);
        }
    }
}
