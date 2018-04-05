using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("IdentityManager2.Tests")]

namespace IdentityManager2.Core.Metadata
{
    public class UserMetadata
    {
        public UserMetadata()
        {
            CreateProperties = UpdateProperties = Enumerable.Empty<PropertyMetadata>();
        }

        public bool SupportsCreate { get; set; }
        public bool SupportsDelete { get; set; }
        public bool SupportsClaims { get; set; }

        public IEnumerable<PropertyMetadata> CreateProperties { get; set; }
        public IEnumerable<PropertyMetadata> UpdateProperties { get; set; }

        internal void Validate()
        {
            if (CreateProperties == null) CreateProperties = Enumerable.Empty<PropertyMetadata>();

            foreach (var prop in CreateProperties) prop.Validate();

            var createTypes = CreateProperties.Select(x => x.Name).Distinct();

            if (createTypes.Count() < CreateProperties.Count())
            {
                var query =
                    from t in createTypes
                    let props = (from p in CreateProperties where p.Name == t select p)
                    where props.Count() > 1
                    select t;

                var names = query.Distinct().Aggregate((x, y) => x + ", " + y);

                throw new InvalidOperationException("Duplicate CreateProperties Types registered: " + names);
            }

            if (UpdateProperties == null) UpdateProperties = Enumerable.Empty<PropertyMetadata>();

            foreach (var prop in UpdateProperties) prop.Validate();

            var updateTypes = UpdateProperties.Select(x => x.Name).Distinct();

            if (updateTypes.Count() < UpdateProperties.Count())
            {
                var query =
                    from t in updateTypes
                    let props = (from p in UpdateProperties where p.Name == t select p)
                    where props.Count() > 1
                    select t;

                var names = query.Distinct().Aggregate((x, y) => x + ", " + y);

                throw new InvalidOperationException("Duplicate UpdateProperties Types registered: " + names);
            }
        }
    }
}