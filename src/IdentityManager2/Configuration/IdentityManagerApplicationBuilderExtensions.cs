using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace IdentityManager2.Configuration
{
    public static class IdentityManagerApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseIdentityManager(this IApplicationBuilder app)
        {
            app.UseFileServer(new FileServerOptions
            {
                RequestPath = new PathString("/assets"),
                FileProvider = new EmbeddedFileProvider(typeof(Assets.EmbeddedHtmlResult).Assembly, "IdentityManager2.Assets")
            });

            app.UseMvc();

            return app;
        }
    }
}