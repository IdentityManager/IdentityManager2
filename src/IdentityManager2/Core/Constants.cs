using System;

namespace IdentityManager2.Core
{
    public class Constants
    {
        public const string LocalAuthenticationType = "idmgr.Local";
        public const string BearerAuthenticationType = "idmgr.Bearer";

        public const string AuthorizePath = "/authorize";
        public const string CallbackFragment = "/#/callback/";
        public const string IdMgrClientId = "idmgr";
        public const string IdMgrScope = "idmgr";
        public const string AdminRoleName = "IdentityManagerAdministrator";

        public const string IdMgrAuthPolicy = "idmgr";

        public static readonly TimeSpan DefaultTokenExpiration = TimeSpan.FromHours(10);

        public const string RoutePrefix = "api";
        public const string MetadataRoutePrefix = RoutePrefix + "";
        public const string UserRoutePrefix = RoutePrefix + "/users";
        public const string RoleRoutePrefix = RoutePrefix + "/roles";

        public class ClaimTypes
        {
            public const string Subject = "sub";
            public const string Username = "username";
            public const string Name = "name";
            public const string Password = "password";
            public const string Email = "email";
            public const string Phone = "phone";
            public const string Role = "role";
        }

        public class RouteNames
        {
            public const string GetUsers = "GetUsers";
            public const string GetUser = "GetUser";
            public const string CreateUser = "CreateUser";
            public const string DeleteUser = "DeleteUser";
            public const string UpdateUserProperty = "UpdateUserProperty";
            public const string AddClaim = "AddClaim";
            public const string RemoveClaim = "RemoveClaim";
            public const string AddRole = "AddRole";
            public const string RemoveRole = "RemoveRole";

            public const string GetRoles = "GetRoles";
            public const string GetRole = "GetRole";
            public const string CreateRole = "CreateRole";
            public const string DeleteRole = "DeleteRole";
            public const string UpdateRoleProperty = "UpdateRoleProperty";

            public const string Home = "Home";
            public const string Logout = "Logout";
        }
    }
}
