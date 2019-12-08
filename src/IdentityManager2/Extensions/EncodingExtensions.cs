using System.Text;
using Microsoft.AspNetCore.Authentication;

namespace IdentityManager2.Extensions
{
    public static class EncodingExtensions
    {
        public static string ToBase64UrlEncoded(this string s)
        {
            if (s == null) return null;
            return Base64UrlTextEncoder.Encode(Encoding.UTF8.GetBytes(s));
        }

        public static string FromBase64UrlEncoded(this string s)
        {
            if (s == null) return null;
            return Encoding.UTF8.GetString(Base64UrlTextEncoder.Decode(s));
        }
    }
}