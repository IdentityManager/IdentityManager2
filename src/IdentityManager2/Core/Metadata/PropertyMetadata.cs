using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using IdentityManager2.Extensions;
using IdentityManager2.Resources;
using static System.String;

namespace IdentityManager2.Core.Metadata
{
    public class PropertyMetadata
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public PropertyDataType DataType { get; set; }
        public bool Required { get; set; }

        public void Validate()
        {
            if (IsNullOrWhiteSpace(Type)) throw new ArgumentNullException($"PropertyMetadata::Validate:: {Type} {ExceptionMessages.IsNotAssigned}");
            if (IsNullOrWhiteSpace(Name)) throw new ArgumentNullException($"PropertyMetadata::Validate:: {Name} {ExceptionMessages.IsNotAssigned}");
        }

        public override string ToString()
        {
            var str = new StringBuilder()
                .AppendLine($"Name: {Type}\t")
                .AppendLine($"Display Name: {Name}\t")
                .AppendLine($"Data Type: {DataType.ToString()}\t")
                .AppendLine($"Required: {Required}");

            return str.ToString();
        }

        public static PropertyMetadata FromFunctions<TContainer, TProperty>(
            string name,
            Func<TContainer, TProperty> get,
            Func<TContainer, TProperty, IdentityManagerResult> set,
            string displayName = null,
            PropertyDataType? dataType = null,
            bool? required = null)
        {
            if (IsNullOrWhiteSpace(name)) throw new ArgumentNullException($"PropertyMetadata::FromFunctions::name {ExceptionMessages.IsNotAssigned}");
            if (get == null) throw new ArgumentNullException($"PropertyMetadata::FromFunctions::get {ExceptionMessages.IsNotAssigned}");
            if (set == null) throw new ArgumentNullException($"PropertyMetadata::FromFunctions::set {ExceptionMessages.IsNotAssigned}");

            var meta = new ExpressionPropertyMetadata<TContainer, TProperty>(name, get, set);
            if (name != null) meta.Type = name;
            if (displayName != null) meta.Name = displayName;
            if (dataType != null) meta.DataType = dataType.Value;
            if (required != null) meta.Required = required.Value;

            return meta;
        }

        public static PropertyMetadata FromFunctions<TContainer, TProperty>(
            string name,
            Func<TContainer, Task<TProperty>> get,
            Func<TContainer, TProperty, Task<IdentityManagerResult>> set,
            string displayName = null,
            PropertyDataType? dataType = null,
            bool? required = null)
        {
            if (IsNullOrWhiteSpace(name)) throw new ArgumentNullException($"PropertyMetadata::FromFunctions::name {ExceptionMessages.IsNotAssigned}");
            if (get == null) throw new ArgumentNullException($"PropertyMetadata::FromFunctions::get {ExceptionMessages.IsNotAssigned}");
            if (set == null) throw new ArgumentNullException($"PropertyMetadata::FromFunctions::set {ExceptionMessages.IsNotAssigned}");

            var meta = new AsyncExpressionPropertyMetadata<TContainer, TProperty>(name, get, set);
            if (name != null) meta.Type = name;
            if (displayName != null) meta.Name = displayName;
            if (dataType != null) meta.DataType = dataType.Value;
            if (required != null) meta.Required = required.Value;

            return meta;
        }

        public static PropertyMetadata FromProperty<TContainer>(
            Expression<Func<TContainer, object>> expression,
            string name = null,
            string displayName = null,
            PropertyDataType? dataType = null,
            bool? required = null)
        { 
            if (expression == null) throw new ArgumentNullException("expression");

            if (expression.Body.NodeType != ExpressionType.MemberAccess)
            {
                throw new ArgumentException("Expression must be a member property expression.");
            }

            var memberExpression = (MemberExpression)expression.Body;
            var property = memberExpression.Member as PropertyInfo;
            if (property == null)
            {
                throw new ArgumentException("Expression must be a member property expression.");
            }

            return FromPropertyInfo(property, name, displayName, dataType, required);
        }

        public static PropertyMetadata FromPropertyName<T>(
            string propertyName,
            string name = null,
            string displayName = null,
            PropertyDataType? dataType = null,
            bool? required = null)
        {
            if (IsNullOrWhiteSpace(propertyName)) throw new ArgumentNullException("propertyName");

            var property = typeof(T).GetProperty(propertyName);
            if (property == null)
            {
                throw new ArgumentException(propertyName + " is invalid property on " + typeof(T).FullName);
            }

            return FromPropertyInfo(property, name, displayName, dataType, required);
        }

        public static PropertyMetadata FromPropertyInfo(
            PropertyInfo property,
            string name = null,
            string displayName = null,
            PropertyDataType? dataType = null,
            bool? required = null)
        {
            if (property == null) throw new ArgumentNullException("property");

            if (!property.IsValidAsPropertyMetadata())
            {
                throw new InvalidOperationException(property.Name + " is an invalid property for use as PropertyMetadata");
            }

            return new ReflectedPropertyMetadata(property)
            {
                Type = name ?? property.Name,
                Name = displayName ?? property.GetName(),
                DataType = dataType ?? property.GetPropertyDataType(),
                Required = required ?? property.IsRequired(),
            };
        }

        public static IEnumerable<PropertyMetadata> FromType<T>()
        {
            return FromType(typeof(T), new string[0]);
        }

        public static IEnumerable<PropertyMetadata> FromType<T>(params string[] propertiesToExclude)
        {
            return FromType(typeof(T), propertiesToExclude);
        }

        public static IEnumerable<PropertyMetadata> FromType<T>(params Expression<Func<T, object>>[] propertyExpressionsToExclude)
        {
            var propertiesToExclude = new List<string>();
            foreach (var expression in propertyExpressionsToExclude)
            {
                if (expression.Body.NodeType != ExpressionType.MemberAccess)
                {
                    throw new ArgumentException("Expression must be a member property expression.");
                }

                var memberExpression = (MemberExpression)expression.Body;
                var property = memberExpression.Member as PropertyInfo;
                if (property == null)
                {
                    throw new ArgumentException("Expression must be a member property expression.");
                }

                propertiesToExclude.Add(property.Name);
            }

            return FromType(typeof(T), propertiesToExclude.ToArray());
        }

        public static IEnumerable<PropertyMetadata> FromType(Type type, params string[] propertiesToExclude)
        {
            if (type == null) throw new ArgumentNullException("PropertyMetadata::FromType::type is unassigned");

            var props = new List<PropertyMetadata>();

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly);
            foreach (var property in properties)
            {
                if (!propertiesToExclude.Contains(property.Name, StringComparer.OrdinalIgnoreCase))
                {
                    if (property.IsValidAsPropertyMetadata())
                    {
                        var propMeta = FromPropertyInfo(property);
                        props.Add(propMeta);
                    }
                }
            }

            return props;
        }
    }
}
