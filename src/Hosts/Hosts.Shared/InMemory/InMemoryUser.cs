using System.ComponentModel.DataAnnotations;

namespace Hosts.Shared.InMemory
{
    public class InMemoryUser : InMemoryUserBase
    {
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        public int Age { get; set; }
        public bool IsNice { get; set; }
    }
}
