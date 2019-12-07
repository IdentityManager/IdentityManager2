using System;
using System.ComponentModel.DataAnnotations;

namespace Hosts.Shared.InMemory
{
    public class InMemoryRole
    {
        public InMemoryRole()
        {
            ID = Guid.NewGuid().ToString();
        }

        public string ID { get; set; }

        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
