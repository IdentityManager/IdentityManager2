namespace IdentityManager2.Core.Metadata
{
    public abstract class ExecutablePropertyMetadata : PropertyMetadata
    {
        public abstract string Get(object instance);
        public abstract IdentityManagerResult Set(object instance, string value);
    }
}
