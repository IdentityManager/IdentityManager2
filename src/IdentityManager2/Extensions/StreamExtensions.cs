using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace IdentityManager2.Extensions
{
    public static class StreamExtensions
    {
        public static async Task<string> ReadAsStringAsync(this Stream stream)
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8))
                return await reader.ReadToEndAsync();
        }
    }
}
