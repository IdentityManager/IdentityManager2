using System;

namespace IdentityManager2.Core.Metadata
{
    public class IdentityManagerMetadata
    {
        public UserMetadata UserMetadata { get; set; }
        public RoleMetadata RoleMetadata { get; set; }

        public IdentityManagerMetadata()
        {
            UserMetadata = new UserMetadata();
            RoleMetadata = new RoleMetadata();
        }

        public void Validate()
        {
            if (UserMetadata == null) throw new InvalidOperationException("UserMetadata not assigned.");
            UserMetadata.Validate();
            if (RoleMetadata == null) throw new InvalidOperationException("RoleMetadata not assigned.");
            RoleMetadata.Validate();
        }
    }
}
