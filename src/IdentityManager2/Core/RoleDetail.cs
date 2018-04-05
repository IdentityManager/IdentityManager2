using System.Collections.Generic;

namespace IdentityManager2.Core
{
    public class RoleDetail : RoleSummary
    {
        public IEnumerable<PropertyValue> Properties { get; set; }
    }
}
