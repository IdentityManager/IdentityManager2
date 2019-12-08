using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityManager2.Configuration
{
    public class SecurityConfiguration
    {
        public string HostAuthenticationType { get; set; }
        public string HostChallengeType { get; set; }
        public string AdditionalSignOutType { get; set; }
        
        public string NameClaimType { get; set; }
        public string RoleClaimType { get; set; }
        public string AdminRoleName { get; set; }

        public bool ShowLoginButton { get; set; }

        public SecurityConfiguration()
        {
            NameClaimType = IdentityManagerConstants.ClaimTypes.Name;
            RoleClaimType = IdentityManagerConstants.ClaimTypes.Role;
            AdminRoleName = IdentityManagerConstants.AdminRoleName;

            ShowLoginButton = true;
        }

        internal virtual void Validate()
        {
            if (string.IsNullOrWhiteSpace(HostAuthenticationType))
            {
                throw new Exception("HostAuthenticationType is required.");
            }
            if (string.IsNullOrWhiteSpace(NameClaimType))
            {
                throw new Exception("NameClaimType is required.");
            }
            if (string.IsNullOrWhiteSpace(RoleClaimType))
            {
                throw new Exception("RoleClaimType is required.");
            }
            if (string.IsNullOrWhiteSpace(AdminRoleName))
            {
                throw new Exception("AdminRoleName is required.");
            }
        }

        public virtual void Configure(IServiceCollection services)
        {
        }

        internal virtual async Task SignOut(HttpContext context)
        {
            await context.SignOutAsync(HostAuthenticationType);

            if (!string.IsNullOrWhiteSpace(AdditionalSignOutType))
                await context.SignOutAsync(AdditionalSignOutType);
        }
    }
}