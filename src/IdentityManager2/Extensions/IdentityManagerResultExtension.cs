using System;
using System.Linq;
using IdentityManager2.Api.Models;
using IdentityManager2.Core;

namespace IdentityManager2.Extensions
{
    public static class IdentityManagerResultExtensions
    {
        public static ErrorModel ToError(this IdentityManagerResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            return new ErrorModel
            {
                Errors = result.Errors.ToArray()
            };
        }
    }
}
