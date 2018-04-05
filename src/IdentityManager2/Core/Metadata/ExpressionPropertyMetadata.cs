using System;
using IdentityManager2.Extensions;
using IdentityManager2.Resources;

namespace IdentityManager2.Core.Metadata
{
    public class ExpressionPropertyMetadata<TContainer, TProperty> : ExecutablePropertyMetadata
    {
        private readonly Func<TContainer, TProperty> get;
        private readonly Func<TContainer, TProperty, IdentityManagerResult> set;

        public ExpressionPropertyMetadata(
            string name,
            Func<TContainer, TProperty> get,
            Func<TContainer, TProperty, IdentityManagerResult> set)
            : this(name, name, get, set)
        {
        }

        public ExpressionPropertyMetadata(
            string name,
            string displayName,
            Func<TContainer, TProperty> get,
            Func<TContainer, TProperty, IdentityManagerResult> set)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("ExpressionPropertyMetadata::Ctor::name is unassigned.");
            Name = name;

            if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentNullException("ExpressionPropertyMetadata::Ctor::displayName is unassigned.");
            Name = displayName;

            this.get = get ?? throw new ArgumentNullException("ExpressionPropertyMetadata::Ctor::get is unassigned.");
            this.set = set ?? throw new ArgumentNullException("ExpressionPropertyMetadata::Ctor::set is unassigned.");

            DataType = typeof(TProperty).GetPropertyDataType();
        }

        public override string Get(object instance)
        {
            if (instance == null) throw new ArgumentNullException("ExpressionPropertyMetadata::Get::instance is unassigned.");

            if (DataType == PropertyDataType.Password) return null;

            var value = get((TContainer)instance);
            return value?.ToString();
        }

        public override IdentityManagerResult Set(object instance, string value)
        {
            if (instance == null) throw new ArgumentNullException("ExpressionPropertyMetadata::Set::instance is unassigned.");

            if (string.IsNullOrWhiteSpace(value))
            {
                return set((TContainer)instance, default(TProperty));
            }

            var type = typeof(TProperty);
            type = Nullable.GetUnderlyingType(type) ?? type;
            TProperty convertedValue;

            try
            {
                convertedValue = (TProperty)Convert.ChangeType(value, type);
            }
            catch
            {
                return new IdentityManagerResult(Messages.ConversionFailed);
            }
            return set((TContainer)instance, convertedValue);
        }
    }
}
