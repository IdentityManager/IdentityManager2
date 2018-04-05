using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Host.InMemory
{
    public class InMemoryUserBase
    {
        protected InMemoryUserBase()
        {
            Claims = new HashSet<Claim>();
            Subject = Guid.NewGuid().ToString();
        }

        public string Subject { get; set; }

        [Required]
        public string Username { get; set; }

        [DataType(DataType.Password)]
        [Required]
        public string Password { get; set; }

        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public string Mobile { get; set; }

        public ICollection<Claim> Claims { get; set; }
    }

}
