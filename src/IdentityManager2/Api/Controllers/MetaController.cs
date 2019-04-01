using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityManager2.Api.Models;
using IdentityManager2.Core;
using IdentityManager2.Core.Metadata;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityManager2.Api.Controllers
{
    [Route(Constants.MetadataRoutePrefix)]
    [Authorize(Constants.IdMgrAuthPolicy)]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class MetaController : Controller
    {
        private readonly IIdentityManagerService userManager;
        private IdentityManagerMetadata metadata;

        public MetaController(IIdentityManagerService userManager)
        {
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }
        
        private async Task<IdentityManagerMetadata> GetMetadataAsync()
        {
            if (metadata == null)
            {
                metadata = await userManager.GetMetadataAsync();
                if (metadata == null) throw new InvalidOperationException("GetMetadataAsync returned null");
                metadata.Validate();
            }

            return metadata;
        }

        [Route("")]
        public async Task<IActionResult> Get()
        {
            var meta = await GetMetadataAsync();
            var data = new Dictionary<string, object> {{"currentUser", new {username = User.Identity.Name}}};
            
            var links = new Dictionary<string, object> {["users"] = Url.Link("GetUsers", null)};

            if (meta.RoleMetadata.SupportsListing)
            {
                links["roles"] = Url.Link("GetRoles", null);
            }
            if (meta.UserMetadata.SupportsCreate)
            {
                links["createUser"] = new CreateUserLink(Url, meta.UserMetadata);
            }
            if (meta.RoleMetadata.SupportsCreate)
            {
                links["createRole"] = new CreateRoleLink(Url, meta.RoleMetadata);
            }

            return Ok(new
            {
                Data = data,
                Links = links
            });
        }
    }
}