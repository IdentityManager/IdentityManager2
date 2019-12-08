using IdentityManager2.Assets;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace Microsoft.AspNetCore.Builder
{
    public static class IdentityManagerApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseIdentityManager(this IApplicationBuilder app)
        {
            app.UseFileServer(new FileServerOptions
            {
                RequestPath = new PathString("/assets"),
                FileProvider = new EmbeddedFileProvider(typeof(EmbeddedHtmlResult).Assembly, "IdentityManager2.Assets")
            });

            return app;
        }
    }
}