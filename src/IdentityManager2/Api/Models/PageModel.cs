namespace IdentityManager2.Api.Models
{
    public class PageModel
    {
        public string PathBase { get; set; }
        public string Model { get; set; }
    }

    class PageModelParams
    {
        public string PathBase { get; set; }
        public bool ShowLoginButton { get; set; }
        public string TitleNavBarLinkTarget { get; set; }
    }
}