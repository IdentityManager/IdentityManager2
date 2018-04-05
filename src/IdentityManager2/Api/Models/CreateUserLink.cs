using System;
using System.Collections.Generic;
using IdentityManager2.Core.Metadata;
using IdentityManager2.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace IdentityManager2.Api.Models
{
    public class CreateUserLink : Dictionary<string, object>
    {
        public CreateUserLink(IUrlHelper url, UserMetadata userMetadata)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));
            if (userMetadata == null) throw new ArgumentNullException(nameof(userMetadata));

            this["href"] = url.Link("CreateUser", null);
            this["meta"] = userMetadata.GetCreateProperties();
        }
    }
}