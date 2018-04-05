using System;
using System.Collections.Generic;
using IdentityManager2.Core;
using IdentityManager2.Core.Metadata;
using IdentityManager2.Mappers;
using Microsoft.AspNetCore.Mvc;

namespace IdentityManager2.Api.Models
{
    public class RoleQueryResultResource
    {
        public RoleQueryResultResourceData Data { get; set; }
        public object Links { get; set; }

        public RoleQueryResultResource(QueryResult<RoleSummary> result, IUrlHelper url, RoleMetadata meta)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (url == null) throw new ArgumentNullException(nameof(url));
            if (meta == null) throw new ArgumentNullException(nameof(meta));

            Data = new RoleQueryResultResourceData(result, url, meta);

            var links = new Dictionary<string, object>();
            if (meta.SupportsCreate)
            {
                links["create"] = new CreateRoleLink(url, meta);
            };
            Links = links;
        }
    }

    public class RoleQueryResultResourceData : QueryResult<RoleSummary>
    {
        public new IEnumerable<RoleResultResource> Items { get; set; }

        public RoleQueryResultResourceData(QueryResult<RoleSummary> result, IUrlHelper url, RoleMetadata meta)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (url == null) throw new ArgumentNullException(nameof(url));
            if (meta == null) throw new ArgumentNullException(nameof(meta));

            RoleResultMappers.MapToResultData(result, this);

            foreach (var role in Items)
            {
                var links = new Dictionary<string, string>
                {
                    { "detail", url.Link(Constants.RouteNames.GetRole, new { subject = role.Data.Subject }) }
                };

                if (meta.SupportsDelete)
                {
                    links.Add("delete", url.Link(Constants.RouteNames.DeleteRole, new { subject = role.Data.Subject }));
                }
                role.Links = links;
            }
        }
    }

    public class RoleResultResource
    {
        public RoleSummary Data { get; set; }
        public object Links { get; set; }
    }
}
