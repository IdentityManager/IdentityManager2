using System.ComponentModel.DataAnnotations;
using IdentityManager2.Resources;

namespace IdentityManager2.Core
{
    public class ClaimValue
    {
        [Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "ClaimTypeRequired")]
        public string Type { get; set; }
        [Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "ClaimValueRequired")]
        public string Value { get; set; }
    }
}
