using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IdentityManager2.Api.Models;
using IdentityManager2.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IdentityManager2.Assets
{
    internal class EmbeddedHtmlResult : IActionResult
    {
        private readonly string path;
        private readonly string file;
        private readonly SecurityConfiguration securityConfiguration;

        public EmbeddedHtmlResult(PathString pathBase, string file, SecurityConfiguration securityConfiguration)
        {
            path = pathBase.Value;
            this.file = file;
            this.securityConfiguration = securityConfiguration;
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            var html = AssetManager.LoadResourceString(file,
                new
                {
                    pathBase = path,
                    model = JsonSerializer.Serialize(new PageModelParams
                    {
                        PathBase = path,
                        ShowLoginButton = securityConfiguration.ShowLoginButton
                    })
                });

            context.HttpContext.Response.ContentType = "text/html";
            await context.HttpContext.Response.WriteAsync(html, Encoding.UTF8);
        }
    }
}
