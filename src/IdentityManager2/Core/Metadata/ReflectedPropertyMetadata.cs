using System;
using System.Reflection;
using IdentityManager2.Resources;

namespace IdentityManager2.Core.Metadata
{
    public class ReflectedPropertyMetadata : ExecutablePropertyMetadata
    {
        readonly PropertyInfo property;

        public ReflectedPropertyMetadata() { }

        public ReflectedPropertyMetadata(PropertyInfo property)
        {
            this.property = property ?? throw new ArgumentNullException("property");
        }

        public override string Get(object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("ReflectedPropertyMetadata::Get::instance is unassigned");
            }

            if (DataType == PropertyDataType.Password)
            {
                return null;
            }

            var value = property.GetValue(instance);

            return value?.ToString();
        }

        public override IdentityManagerResult Set(object instance, string value)
        {
            if (instance == null) throw new ArgumentNullException("ReflectedPropertyMetadata::Set::instance is unassigned");

            if (string.IsNullOrWhiteSpace(value))
            {
                property.SetValue(instance, null);
            }
            else
            {
                var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                object convertedValue;

                try
                {
                    convertedValue = Convert.ChangeType(value, type);
                }
                catch
                {
                    return new IdentityManagerResult(Messages.ConversionFailed);
                }

                property.SetValue(instance, convertedValue);
            }

            return IdentityManagerResult.Success;
        }
    }
}
