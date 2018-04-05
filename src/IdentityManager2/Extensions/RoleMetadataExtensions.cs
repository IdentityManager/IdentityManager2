using System;
using System.Collections.Generic;
using System.Linq;
using IdentityManager2.Core.Metadata;

namespace IdentityManager2.Extensions
{
    public static class RoleMetadataExtensions
    {
        public static IEnumerable<PropertyMetadata> GetCreateProperties(this RoleMetadata roleMetadata)
        {
            if (roleMetadata == null) throw new ArgumentNullException("roleMetadata");

            var exclude = roleMetadata.CreateProperties.Select(x => x.Type);
            var additional = roleMetadata.UpdateProperties.Where(x => !exclude.Contains(x.Type) && x.Required);
            return roleMetadata.CreateProperties.Union(additional).ToList();
        }
    }

}
