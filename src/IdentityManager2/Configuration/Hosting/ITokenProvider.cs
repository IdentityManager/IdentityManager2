namespace IdentityManager2.Configuration.Hosting
{
    public interface ITokenProvider<T>
    {
        string Generate(T model);
        T Validate(string data);
    }
}