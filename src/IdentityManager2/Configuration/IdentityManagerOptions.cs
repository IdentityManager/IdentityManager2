using System;

namespace IdentityManager2.Configuration
{
    public class IdentityManagerOptions
    {
        public SecurityConfiguration SecurityConfiguration { get; set; } = new LocalhostSecurityConfiguration();

        internal void Validate()
        {
            if (SecurityConfiguration == null)
            {
                throw new Exception("SecurityConfiguration is required.");
            }

            SecurityConfiguration.Validate();
        }
    }
}