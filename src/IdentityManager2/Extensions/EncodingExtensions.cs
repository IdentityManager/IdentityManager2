using System;
using System.Text;

namespace IdentityManager2.Extensions
{
    public static class EncodingExtensions
    {
        public static string ToBase64UrlEncoded(this string s)
        {
            if (s == null) return null;

            var bytes = Encoding.UTF8.GetBytes(s);

            return bytes.ToBase64UrlEncoded();
        }

        public static string ToBase64UrlEncoded(this byte[] bytes)
        {
            if (bytes == null) return null;
            
            var s = Convert.ToBase64String(bytes);
            s = s.Split('=')[0]; // Remove any trailing '='s
            s = s.Replace('+', '-'); // 62nd char of encoding
            s = s.Replace('/', '_'); // 63rd char of encoding

            return s;
        }

        public static string FromBase64UrlEncoded(this string s)
        {
            if (s == null) return null;

            var bytes = s.FromBase64UrlEncodedBytes();
            return Encoding.UTF8.GetString(bytes);
        }

        public static byte[] FromBase64UrlEncodedBytes(this string s)
        {
            if (s == null) return null;

            s = s.Replace('-', '+'); // 62nd char of encoding
            s = s.Replace('_', '/'); // 63rd char of encoding

            switch (s.Length % 4) // Pad with trailing '='s
            {
                case 0: break; // No pad chars in this case
                case 2: s += "=="; break; // Two pad chars
                case 3: s += "="; break; // One pad char
                default: throw new Exception("Illegal base64url string!");
            }

            return Convert.FromBase64String(s);
        }
    }
}
