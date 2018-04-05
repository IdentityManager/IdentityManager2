using System;
using System.Collections.Generic;
using System.Linq;
using IdentityManager2.Core;
using IdentityManager2.Core.Metadata;
using IdentityManager2.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace IdentityManager2.Api.Models
{
    public class UserDetailResource
    {
        public UserDetailDataResource Data { get; set; }
        public object Links { get; set; }

        public UserDetailResource(UserDetail user, IUrlHelper url, IdentityManagerMetadata idmMeta, RoleSummary[] roles)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (url == null) throw new ArgumentNullException(nameof(url));
            if (idmMeta == null) throw new ArgumentNullException(nameof(idmMeta));

            Data = new UserDetailDataResource(user, url, idmMeta, roles);

            var links = new Dictionary<string, string>();
            if (idmMeta.UserMetadata.SupportsDelete)
            {
                links["delete"] = url.Link(Constants.RouteNames.DeleteUser, new { subject = user.Subject });
            }
            Links = links;
        }
    }

    public class UserDetailDataResource : Dictionary<string, object>
    {
        public UserDetailDataResource(UserDetail user, IUrlHelper url, IdentityManagerMetadata meta, RoleSummary[] roles)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (url == null) throw new ArgumentNullException(nameof(url));
            if (meta == null) throw new ArgumentNullException(nameof(meta));
            if (roles == null) { throw new ArgumentNullException(nameof(roles)); }

            this["username"] = user.Username;
            this["name"] = user.Name;
            this["subject"] = user.Subject;

            if (user.Properties != null)
            {
                var props =
                    from p in user.Properties
                    let m = (from m in meta.UserMetadata.UpdateProperties where m.Type == p.Type select m).SingleOrDefault()
                    where m != null
                    select new
                    {
                        Data = m.Convert(p.Value),
                        Meta = m,
                        Links = new
                        {
                            update = url.Link(Constants.RouteNames.UpdateUserProperty,
                                new
                                {
                                    subject = user.Subject,
                                    type = p.Type.ToBase64UrlEncoded()
                                }
                            ),
                        }
                    };

                if (props.Any())
                {
                    this["properties"] = props.ToArray();
                }
            }

            if (user.Claims != null)
            {
                var roleClaims = user.Claims.Where(x => x.Type == meta.RoleMetadata.RoleClaimType);
                var query =
                    from r in roles
                    orderby r.Name
                    select new
                    {
                        data = roleClaims.Any(x => x.Value == r.Name),
                        meta = new
                        {
                            type = r.Name,
                            description = r.Description,
                        },
                        links = new
                        {
                            add = url.Link(Constants.RouteNames.AddRole, new { subject = user.Subject, role = r.Name.ToBase64UrlEncoded() }),
                            remove = url.Link(Constants.RouteNames.RemoveRole, new { subject = user.Subject, role = r.Name.ToBase64UrlEncoded() })
                        }
                    };
                this["roles"] = query.ToArray();
            }

            if (meta.UserMetadata.SupportsClaims && user.Claims != null)
            {
                var claims =
                    from c in user.Claims.ToArray()
                    select new
                    {
                        Data = c,
                        Links = new
                        {
                            delete = url.Link(Constants.RouteNames.RemoveClaim, new
                            {
                                subject = user.Subject,
                                type = c.Type.ToBase64UrlEncoded(),
                                value = c.Value.ToBase64UrlEncoded()
                            })
                        }
                    };

                this["claims"] = new
                {
                    Data = claims.ToArray(),
                    Links = new
                    {
                        create = url.Link(Constants.RouteNames.AddClaim, new { subject = user.Subject })
                    }
                };
            }
        }
    }
   
}
