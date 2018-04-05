namespace Microsoft.Extensions.DependencyInjection
{
    public interface IIdentityManagerBuilder
    {
        IServiceCollection Services { get; }
    }
}