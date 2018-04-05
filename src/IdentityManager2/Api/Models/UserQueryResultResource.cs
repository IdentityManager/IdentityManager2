using System;
using System.Collections.Generic;
using IdentityManager2.Core;
using IdentityManager2.Core.Metadata;
using IdentityManager2.Mappers;
using Microsoft.AspNetCore.Mvc;

namespace IdentityManager2.Api.Models
{
    public class UserQueryResultResource
    {
        public UserQueryResultResourceData Data { get; set; }
        public object Links { get; set; }

        public UserQueryResultResource(QueryResult<UserSummary> result, IUrlHelper url, UserMetadata meta)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (url == null) throw new ArgumentNullException(nameof(url));
            if (meta == null) throw new ArgumentNullException(nameof(meta));

            Data = new UserQueryResultResourceData(result, url, meta);

            var links = new Dictionary<string, object>();
            if (meta.SupportsCreate)
            {
                links["create"] = new CreateUserLink(url, meta);
            }
            
            Links = links;
        }
    }

    public class UserQueryResultResourceData : QueryResult<UserSummary>
    {
        public new IEnumerable<UserResultResource> Items { get; set; }

        public UserQueryResultResourceData(QueryResult<UserSummary> result, IUrlHelper url, UserMetadata meta)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (url == null) throw new ArgumentNullException(nameof(url));
            if (meta == null) throw new ArgumentNullException(nameof(meta));

            UserResultMappers.MapToResultData(result, this);

            foreach (var user in Items)
            {
                var links = new Dictionary<string, string>
                {
                    {"detail", url.Link(Constants.RouteNames.GetUser, new {subject = user.Data.Subject})}
                };
                if (meta.SupportsDelete)
                {
                    links.Add("delete", url.Link(Constants.RouteNames.DeleteUser, new {subject = user.Data.Subject}));
                }

                user.Links = links;
            }
        }
    }
    
    public class UserResultResource
    {
        public UserSummary Data { get; set; }
        public object Links { get; set; }
    }
}