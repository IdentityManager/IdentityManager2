using System;
using System.Threading.Tasks;
using IdentityManager2.Extensions;
using IdentityManager2.Resources;

namespace IdentityManager2.Core.Metadata
{
    public class AsyncExpressionPropertyMetadata<TContainer, TProperty> : AsyncExecutablePropertyMetadata
    {
        private readonly Func<TContainer, Task<TProperty>> get;
        private readonly Func<TContainer, TProperty, Task<IdentityManagerResult>> set;

        public AsyncExpressionPropertyMetadata(
            string name,
            Func<TContainer, Task<TProperty>> get,
            Func<TContainer, TProperty, Task<IdentityManagerResult>> set)
            : this(name, name, get, set)
        {
        }

        public AsyncExpressionPropertyMetadata(
            string name,
            string displayName,
            Func<TContainer, Task<TProperty>> get,
            Func<TContainer, TProperty, Task<IdentityManagerResult>> set)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("AsyncExpressionPropertyMetadata::Ctor::name is unassigned.");
            Name = name;

            if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentNullException("AsyncExpressionPropertyMetadata::Ctor::displayName is unassigned.");
            Name = displayName;

            this.get = get ?? throw new ArgumentNullException("AsyncExpressionPropertyMetadata::Ctor::get is unassigned.");
            this.set = set ?? throw new ArgumentNullException("AsyncExpressionPropertyMetadata::Ctor::set is unassigned.");

            DataType = typeof(TProperty).GetPropertyDataType();
        }

        public override async Task<string> Get(object instance)
        {
            if (instance == null) throw new ArgumentNullException("AsyncExpressionPropertyMetadata::Get::instance is unassigned.");

            if (DataType == PropertyDataType.Password) return null;

            var value = await get((TContainer)instance);
            return value?.ToString();
        }

        public override Task<IdentityManagerResult> Set(object instance, string value)
        {
            if (instance == null) throw new ArgumentNullException("AsyncExpressionPropertyMetadata::Set::instance is unassigned.");

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
                return Task.FromResult(new IdentityManagerResult(Messages.ConversionFailed));
            }
            return set((TContainer)instance, convertedValue);
        }
    }
}