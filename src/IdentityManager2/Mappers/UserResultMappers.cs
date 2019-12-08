using System.Linq;
using IdentityManager2.Api.Models;
using IdentityManager2.Core;

namespace IdentityManager2.Mappers
{
    public static class UserResultMappers
    {
        public static void MapToResultData(QueryResult<UserSummary> result, UserQueryResultResourceData data)
        {
            data.Count = result.Count;
            data.Filter = result.Filter;
            data.Start = result.Start;
            data.Total = result.Total;

            data.Items = result.Items
                .Select(x => new UserResultResource {Data = x, Links = x})
                .ToList();
        }
    }
}