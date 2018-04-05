using System.Collections.Generic;

namespace IdentityManager2.Core
{
    public class QueryResult<T>
    {
        public int Start { get; set; }
        public int Count { get; set; }
        public int Total { get; set; }
        public string Filter { get; set; }
        public IList<T> Items { get; set; }

        public QueryResult()
        {
            Items = new List<T>();
        }
    }
}
