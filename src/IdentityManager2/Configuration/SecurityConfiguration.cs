using System;
using System.Threading.Tasks;
using IdentityManager2.Core;
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

        public string BearerAuthenticationType { get; set; }
        public TimeSpan TokenExpiration { get; set; }

        public string NameClaimType { get; set; }
        public string RoleClaimType { get; set; }
        public string AdminRoleName { get; set; }

        public bool ShowLoginButton { get; set; }

        public SecurityConfiguration()
        {
            BearerAuthenticationType = Constants.BearerAuthenticationType;
            TokenExpiration = Constants.DefaultTokenExpiration;
            
            NameClaimType = Constants.ClaimTypes.Name;
            RoleClaimType = Constants.ClaimTypes.Role;
            AdminRoleName = Constants.AdminRoleName;

            ShowLoginButton = true;
        }

        internal virtual void Validate()
        {
            if (string.IsNullOrWhiteSpace(HostAuthenticationType))
            {
                throw new Exception("HostAuthenticationType is required.");
            }
            if (string.IsNullOrWhiteSpace(BearerAuthenticationType))
            {
                throw new Exception("BearerAuthenticationType is required.");
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