using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityManager2;
using IdentityManager2.Core;
using IdentityManager2.Core.Metadata;
using IdentityManager2.Extensions;
using IdentityManager2.Resources;

namespace Host.InMemory
{
    public class InMemoryIdentityManagerService : IIdentityManagerService
    {
        private readonly ICollection<InMemoryUser> users;
        private readonly ICollection<InMemoryRole> roles;

        public InMemoryIdentityManagerService(ICollection<InMemoryUser> users, ICollection<InMemoryRole> roles)
        {
            this.users = users ?? throw new ArgumentNullException(nameof(users));
            this.roles = roles ?? throw new ArgumentNullException(nameof(roles));
        }

        private IdentityManagerMetadata metadata;

        public IdentityManagerMetadata GetMetadata()
        {
            if (metadata == null)
            {
                var createprops = new List<PropertyMetadata>()
                {
                    PropertyMetadata.FromProperty<InMemoryUser>(x => x.Username, name:Constants.ClaimTypes.Username, required:true),
                };

                var updateprops = new List<PropertyMetadata>();
                updateprops.AddRange(new PropertyMetadata[]{
                    PropertyMetadata.FromProperty<InMemoryUser>(x => x.Username, name:Constants.ClaimTypes.Username, required:true),
                    PropertyMetadata.FromPropertyName<InMemoryUser>("Password", name:Constants.ClaimTypes.Password, required:true),
                    PropertyMetadata.FromFunctions<InMemoryUser, string>(Constants.ClaimTypes.Name, u => GetName(u), SetName, displayName:"DisplayName", required:true),
                });
                updateprops.AddRange(PropertyMetadata.FromType<InMemoryUser>());
                updateprops.AddRange(new PropertyMetadata[]{
                    PropertyMetadata.FromPropertyName<InMemoryUser>("Mobile"),
                    PropertyMetadata.FromPropertyName<InMemoryUser>("Email", dataType:PropertyDataType.Email),
                    new PropertyMetadata {
                        Name = "Is Administrator",
                        Type = "role.admin",
                        DataType = PropertyDataType.Boolean,
                        Required = true,
                    },
                    new PropertyMetadata {
                        Name = "Gravatar Url",
                        Type = "gravatar",
                        DataType = PropertyDataType.Url,
                    }
                });

                var roleCreateProps = new List<PropertyMetadata>
                {
                    PropertyMetadata.FromProperty<InMemoryRole>(x => x.Name)
                };
                var roleUpdateProps = new List<PropertyMetadata>
                {
                    PropertyMetadata.FromProperty<InMemoryRole>(x => x.Description, name: "description")
                };

                metadata = new IdentityManagerMetadata()
                {
                    UserMetadata = new UserMetadata
                    {
                        SupportsCreate = true,
                        SupportsDelete = true,
                        SupportsClaims = true,
                        CreateProperties = createprops,
                        UpdateProperties = updateprops
                    },
                    RoleMetadata = new RoleMetadata
                    {
                        RoleClaimType = Constants.ClaimTypes.Role,
                        SupportsCreate = true,
                        SupportsDelete = true,
                        CreateProperties = roleCreateProps,
                        UpdateProperties = roleUpdateProps
                    }
                };
            }
            return metadata;
        }

        private string GetName(InMemoryUser user)
        { 
            if(user == null) throw new ArgumentNullException();
        
            return user.Claims.GetValue(Constants.ClaimTypes.Name);
        }

        private IdentityManagerResult SetName(InMemoryUser user, string value)
        {
            if (user == null) throw new ArgumentNullException("SetName::" + string.Format(ExceptionMessages.IsNotAssigned, user));
            if (value == null) throw new ArgumentNullException("SetName::" + string.Format(ExceptionMessages.IsNotAssigned, value));

            user.Claims.SetValue(Constants.ClaimTypes.Name, value);
            return IdentityManagerResult.Success;
        }

        public Task<IdentityManagerMetadata> GetMetadataAsync()
        {
            return Task.FromResult(GetMetadata());
        }

        public Task<IdentityManagerResult<CreateResult>> CreateUserAsync(IEnumerable<PropertyValue> properties)
        {
            var errors = ValidateUserProperties(properties);
            if (errors.Any()) return Task.FromResult(new IdentityManagerResult<CreateResult>(errors.ToArray()));
            
            var user = new InMemoryUser();
            var createPropsMeta = GetMetadata().UserMetadata.GetCreateProperties();
            foreach (var prop in  properties)
            {
                var result = SetUserProperty(createPropsMeta, user, prop.Type, prop.Value);
                if (!result.IsSuccess)
                {
                    return Task.FromResult(new IdentityManagerResult<CreateResult>(result.Errors.ToArray()));
                }
            }

            if (users.Any(x => x.Username.Equals(user.Username, StringComparison.OrdinalIgnoreCase)))
            {
                return Task.FromResult(new IdentityManagerResult<CreateResult>("Username already in use."));
            }

            users.Add(user);

            return Task.FromResult(new IdentityManagerResult<CreateResult>(new CreateResult() { Subject = user.Subject }));
        }

        public Task<IdentityManagerResult> DeleteUserAsync(string subject)
        {
            var user = users.SingleOrDefault(x => x.Subject == subject);
            if (user != null) users.Remove(user);

            return Task.FromResult(IdentityManagerResult.Success);
        }

        public Task<IdentityManagerResult<QueryResult<UserSummary>>> QueryUsersAsync(string filter, int start, int count)
        {
            var query =
                from u in users
                select u;
            if (!string.IsNullOrWhiteSpace(filter))
            {
                filter = filter.ToLower();
                query =
                    from u in query
                    let names = (from c in u.Claims where c.Type == Constants.ClaimTypes.Name select c.Value.ToLower())
                    where
                        u.Username.ToLower().Contains(filter) ||
                        names.Contains(filter)
                    select u;
            }

            var items =
                from u in query.Distinct()
                select new UserSummary
                {
                    Subject = u.Subject,
                    Username = u.Username,
                    Name = u.Claims.Where(x => x.Type == Constants.ClaimTypes.Name).Select(x => x.Value).FirstOrDefault(),
                };
            var total = items.Count();

            var result = items.Skip(start).Take(count);
            return Task.FromResult(new IdentityManagerResult<QueryResult<UserSummary>>(new QueryResult<UserSummary>
            {
                Filter = filter,
                Start = start,
                Count = result.Count(),
                Items = result.ToList(),
                Total = total
            }));
        }

        public async Task<IdentityManagerResult<UserDetail>> GetUserAsync(string subject)
        {
            var user = users.SingleOrDefault(x => x.Subject == subject);
            if (user == null) return new IdentityManagerResult<UserDetail>((UserDetail)null);
            

            var props = new List<PropertyValue>();
            foreach (var prop in GetMetadata().UserMetadata.UpdateProperties)
            {
                props.Add(new PropertyValue
                {
                    Type = prop.Type,
                    Value = await GetUserProperty(prop, user)
                });
            }

            return new IdentityManagerResult<UserDetail>(new UserDetail
            {
                Subject = user.Subject,
                Username = user.Username,
                Name = user.Claims.GetValue(Constants.ClaimTypes.Name),
                Properties = props,
                Claims = user.Claims.Select(x => new ClaimValue { Type = x.Type, Value = x.Value })
            });
        }

        public Task<IdentityManagerResult> SetUserPropertyAsync(string subject, string type, string value)
        {
            var user = users.SingleOrDefault(x => x.Subject == subject);
            if (user == null) return Task.FromResult(new IdentityManagerResult("No user found"));

            var errors = ValidateUserProperty(type, value);
            if (errors.Any()) return Task.FromResult(new IdentityManagerResult(errors.ToArray()));
            
            var result = SetUserProperty(GetMetadata().UserMetadata.UpdateProperties, user, type, value);
            return Task.FromResult(result);
        }

        public Task<IdentityManagerResult> AddUserClaimAsync(string subject, string type, string value)
        {
            var user = users.SingleOrDefault(x => x.Subject == subject);
            if (user == null) return Task.FromResult(new IdentityManagerResult("No user found"));
            
            user.Claims.AddClaim(type, value);

            return Task.FromResult(IdentityManagerResult.Success);
        }

        public Task<IdentityManagerResult> RemoveUserClaimAsync(string subject, string type, string value)
        {
            var user = users.SingleOrDefault(x => x.Subject == subject);
            if (user == null) return Task.FromResult(new IdentityManagerResult("No user found"));
            
            user.Claims.RemoveClaims(type, value);

            return Task.FromResult(IdentityManagerResult.Success);
        }

        private async Task<string> GetUserProperty(PropertyMetadata property, InMemoryUser user)
        {
            if (property.TryGet(user, out var value)) return await value;
            
            switch (property.Type)
            {
                case "role.admin":
                    return user.Claims.HasValue(Constants.ClaimTypes.Role, "admin").ToString().ToLower();
                case "gravatar":
                    return user.Claims.GetValue("gravatar");
            }

            throw new Exception("Invalid property type " + property.Type);
        }

        public Task<string> GetUserPropertyValue(PropertyMetadata property, InMemoryUser user)
        {
            return GetUserProperty(property, user);
        }

        private IdentityManagerResult SetUserProperty(IEnumerable<PropertyMetadata> propsMeta, InMemoryUser user, string type, string value)
        {
            if (propsMeta.TrySet(user, type, value, out var result)) return result;
            
            switch (type)
            {
                case "role.admin":
                    {
                        var val = bool.Parse(value);
                        if (val) user.Claims.AddClaim(Constants.ClaimTypes.Role, "admin");
                        else user.Claims.RemoveClaim(Constants.ClaimTypes.Role, "admin");
                    }
                    break;
                case "gravatar":
                    {
                        user.Claims.SetValue("gravatar", value);
                    }
                    break;
                default:
                    throw new InvalidOperationException("Invalid Property Type : " + type);
            }

            return IdentityManagerResult.Success;
        }

        private IEnumerable<string> ValidateUserProperties(IEnumerable<PropertyValue> properties)
        {
            return properties.Select(x => ValidateUserProperty(x.Type, x.Value)).Aggregate((x, y) => x.Concat(y));
        }

        private IEnumerable<string> ValidateUserProperty(string type, string value)
        {
            switch (type)
            {
                case Constants.ClaimTypes.Username:
                    {
                        if (users.Any(x => x.Username == value)) return new[] { "That Username is already in use" };
                    }
                    break;
                case Constants.ClaimTypes.Password:
                    {
                        if (value.Length < 3) return new[] { "Password must have at least 3 characters" };
                    }
                    break;
            }

            return Enumerable.Empty<string>();
        }

        public Task<IdentityManagerResult<CreateResult>> CreateRoleAsync(IEnumerable<PropertyValue> properties)
        {
            var errors = ValidateRoleProperties(properties);
            if (errors.Any()) return Task.FromResult(new IdentityManagerResult<CreateResult>(errors.ToArray()));
            

            var role = new InMemoryRole();
            var createPropsMeta = GetMetadata().RoleMetadata.GetCreateProperties();
            foreach (var prop in properties)
            {
                var result = SetRoleProperty(createPropsMeta, role, prop.Type, prop.Value);
                if (!result.IsSuccess) return Task.FromResult(new IdentityManagerResult<CreateResult>(result.Errors.ToArray()));
                
            }

            if (roles.Any(x => x.Name.Equals(role.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return Task.FromResult(new IdentityManagerResult<CreateResult>("Role name already in use."));
            }

            roles.Add(role);

            return Task.FromResult(new IdentityManagerResult<CreateResult>(new CreateResult() { Subject = role.ID }));
        }

        public Task<IdentityManagerResult> DeleteRoleAsync(string subject)
        {
            var role = roles.SingleOrDefault(x => x.ID == subject);
            if (role != null) roles.Remove(role);
            
            return Task.FromResult(IdentityManagerResult.Success);
        }

        public Task<IdentityManagerResult<QueryResult<RoleSummary>>> QueryRolesAsync(string filter, int start, int count)
        {
            var query =
                from r in roles
                select r;
            if (!string.IsNullOrWhiteSpace(filter))
            {
                filter = filter.ToLower();
                query =
                    from r in query
                    where
                        r.Name.ToLower().Contains(filter) ||
                        (r.Description != null && r.Description.ToLower().Contains(filter))
                    select r;
            }

            var items =
                from r in query.Distinct()
                select new RoleSummary
                {
                    Subject = r.ID,
                    Name = r.Name,
                    Description = r.Description
                };
            var total = items.Count();

            var result = items;
            if (start >= 0 && count >= 0)
            {
                result = items.Skip(start).Take(count);
                count = result.Count();
            }
            else
            {
                start = 0;
                count = total;
            }

            var fromResult = Task.FromResult(new IdentityManagerResult<QueryResult<RoleSummary>>(new QueryResult<RoleSummary>
            {
                Filter = filter,
                Start = start,
                Count = result.Count(),
                Items = result.ToList(),
                Total = total,
            }));

            return fromResult;
        }

        public async Task<IdentityManagerResult<RoleDetail>> GetRoleAsync(string subject)
        {
            var role = roles.SingleOrDefault(x => x.ID == subject);
            if (role == null) return new IdentityManagerResult<RoleDetail>((RoleDetail)null);
            

            var props = new List<PropertyValue>();
            foreach (var prop in GetMetadata().RoleMetadata.UpdateProperties)
            {
                props.Add(new PropertyValue
                {
                    Type = prop.Type,
                    Value = await GetRoleProperty(prop, role)
                });
            }

            var detail = new RoleDetail
            {
                Subject = role.ID,
                Name = role.Name,
                Description = role.Description,
                Properties = props
            };

            return new IdentityManagerResult<RoleDetail>(detail);
        }

        public Task<IdentityManagerResult> SetRolePropertyAsync(string subject, string type, string value)
        {
            var role = roles.SingleOrDefault(x => x.ID == subject);
            if (role == null) return Task.FromResult(new IdentityManagerResult("No role found"));
            
            var errors = ValidateRoleProperty(type, value);
            if (errors.Any()) return Task.FromResult(new IdentityManagerResult(errors.ToArray()));
            
            var result = SetRoleProperty(GetMetadata().RoleMetadata.UpdateProperties, role, type, value);
            return Task.FromResult(result);
        }

        private async Task<string> GetRoleProperty(PropertyMetadata property, InMemoryRole role)
        {
            if (property.TryGet(role, out var value)) return await value;
            throw new Exception("Invalid property type " + property.Type);
        }

        private IdentityManagerResult SetRoleProperty(IEnumerable<PropertyMetadata> propsMeta, InMemoryRole role, string type, string value)
        {
            if (propsMeta.TrySet(role, type, value, out var result)) return result;
            throw new InvalidOperationException("Invalid Property Type : " + type);
        }

        private IEnumerable<string> ValidateRoleProperties(IEnumerable<PropertyValue> properties)
        {
            return properties.Select(x => ValidateRoleProperty(x.Type, x.Value)).Aggregate((x, y) => x.Concat(y));
        }

        private IEnumerable<string> ValidateRoleProperty(string type, string value)
        {

            return Enumerable.Empty<string>();
        }
    }

}
