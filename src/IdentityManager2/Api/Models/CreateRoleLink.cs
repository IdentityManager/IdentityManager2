using System;
using System.Collections.Generic;
using IdentityManager2.Core;
using IdentityManager2.Core.Metadata;
using IdentityManager2.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace IdentityManager2.Api.Models
{
    public class CreateRoleLink : Dictionary<string, object>
    {
        public CreateRoleLink(IUrlHelper url, RoleMetadata roleMetadata)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));
            if (roleMetadata == null ) throw new ArgumentNullException(nameof(roleMetadata));

            this["href"] = url.Link(Constants.RouteNames.CreateRole, null);
            this["meta"] = roleMetadata.GetCreateProperties();
        }
    }
}
