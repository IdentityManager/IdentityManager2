using System.Threading.Tasks;

namespace IdentityManager2.Core.Metadata
{
    public abstract class AsyncExecutablePropertyMetadata : PropertyMetadata
    {
        public abstract Task<string> Get(object instance);
        public abstract Task<IdentityManagerResult> Set(object instance, string value);
    }
}