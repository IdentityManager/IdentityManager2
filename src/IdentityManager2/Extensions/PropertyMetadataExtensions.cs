using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityManager2.Core;
using IdentityManager2.Core.Metadata;
using IdentityManager2.Resources;
using static System.Boolean;
using static System.String;

namespace IdentityManager2.Extensions
{
    public static class PropertyMetadataExtensions
    {
        public static IEnumerable<string> Validate(
            this IEnumerable<PropertyMetadata> propertiesMetadata,
            IEnumerable<PropertyValue> properties)
        {
            if (propertiesMetadata == null) throw new ArgumentNullException("propertiesMetadata");
            properties = properties ?? Enumerable.Empty<PropertyValue>();

            var errors = new List<string>();

            var crossQuery =
                from m in propertiesMetadata
                from p in properties
                where m.Type == p.Type
                let e = m.Validate(p.Value)
                where e != null
                select e;

            errors.AddRange(crossQuery);

            var metaTypes = propertiesMetadata.Select(x => x.Type);
            var propTypes = properties.Select(x => x.Type);

            var more = propTypes.Except(metaTypes);
            if (more.Any())
            {
                var types = more.Aggregate((x, y) => x + ", " + y);
                errors.Add(Format(Messages.UnrecognizedProperties, types));
            }

            var less = metaTypes.Except(propTypes);
            if (less.Any())
            {
                var types = less.Aggregate((x, y) => x + ", " + y);
                errors.Add(Format(Messages.MissingRequiredProperties, types));
            }

            return errors;
        }

        public static string Validate(this PropertyMetadata property, string value)
        {
            if (property == null) throw new ArgumentNullException("property");

            if (property.Required && IsNullOrWhiteSpace(value))
            {
                return Format(Messages.IsRequired, property.Type);
            }
            if (!IsNullOrWhiteSpace(value))
            {
                if (property.DataType == PropertyDataType.Boolean)
                {
                    if (!TryParse(value, out var _))
                    {
                        return Format(Messages.InvalidBoolean, property.Type);
                    }
                }

                if (property.DataType == PropertyDataType.Email)
                {
                    if (!value.Contains("@"))
                    {
                        return Format(Messages.InvalidEmail, property.Type);
                    }
                }

                if (property.DataType == PropertyDataType.Number)
                {
                    if (!double.TryParse(value, out var _))
                    {
                        return Format(Messages.InvalidNumber, property.Type);
                    }
                }

                if (property.DataType == PropertyDataType.Url)
                {
                    if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
                        uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                    {
                        return Format(Messages.InvalidUrl, property.Type);
                    }
                }
            }

            return null;
        }

        public static bool TrySet(this IEnumerable<PropertyMetadata> properties, object instance, string name, string value, out Task<IdentityManagerResult> result)
        {
            if (properties == null) throw new ArgumentNullException("properties");
            result = null;

            if (properties.SingleOrDefault(x => x.Type == name) is ExecutablePropertyMetadata executableProperty)
            {
                return executableProperty.TrySet(instance, value, out result);
            }

            if (properties.SingleOrDefault(x => x.Type == name) is AsyncExecutablePropertyMetadata asyncExecutableProperty)
            {
                return asyncExecutableProperty.TrySet(instance, value, out result);
            }

            return false;
        }

        public static bool TrySet(this PropertyMetadata property, object instance, string value, out Task<IdentityManagerResult> result)
        {
            if (property == null) throw new ArgumentNullException("property");
            result = null;

            if (property is ExecutablePropertyMetadata executableProperty)
            {
                result = Task.FromResult(executableProperty.Set(instance, value));
                
                return true;
            }

            if (property is AsyncExecutablePropertyMetadata asyncExecutableProperty)
            {
                result = asyncExecutableProperty.Set(instance, value);

                return true;
            }

            return false;
        }

        public static bool TryGet(this PropertyMetadata property, object instance, out Task<string> value)
        {
            if (property == null) throw new ArgumentNullException("property");

            if (property is ExecutablePropertyMetadata executableProperty)
            {
                value = Task.FromResult(executableProperty.Get(instance));
                return true;
            }

            if (property is AsyncExecutablePropertyMetadata asyncExecutableProperty)
            {
                value = asyncExecutableProperty.Get(instance);
                return true;
            }

            value = null;
            return false;
        }

        public static object Convert(this PropertyMetadata property, string value)
        {
            if (property == null) throw new ArgumentNullException("property");

            if (IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (property.DataType == PropertyDataType.Boolean)
            {
                return Parse(value);
            }

            if (property.DataType == PropertyDataType.Number)
            {
                return double.Parse(value);
            }

            return value;
        }
    }
}
