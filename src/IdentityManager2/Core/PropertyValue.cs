using System.ComponentModel.DataAnnotations;
using IdentityManager2.Resources;

namespace IdentityManager2.Core
{
    public class  PropertyValue
    {
        [Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "PropertyTypeRequired")]
        public string Type { get; set; }
        public string Value { get; set; }
    }
}
