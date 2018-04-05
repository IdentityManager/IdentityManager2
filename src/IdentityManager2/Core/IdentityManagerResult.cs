using System.Collections.Generic;
using System.Linq;

namespace IdentityManager2.Core
{
    public class IdentityManagerResult
    {
        public IList<string> Errors { get; set; }
        public bool IsSuccess { get; set; }

        public static readonly IdentityManagerResult Success = new IdentityManagerResult();

        public IdentityManagerResult()
        {
            IsSuccess = Errors == null || !Errors.Any();
        }

        public IdentityManagerResult(params string[] errors) : this()
        {
            if (errors.Length > 0)
            {
                IsSuccess = false;
            }
            Errors = errors;
        }
    }
}
