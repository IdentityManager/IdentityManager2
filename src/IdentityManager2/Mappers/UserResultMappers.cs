using AutoMapper;
using IdentityManager2.Api.Models;
using IdentityManager2.Core;

namespace IdentityManager2.Mappers
{
    public static class UserResultMappers
    {
        internal static IMapper Mapper { get; }

        static UserResultMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<UserResultMapperProfile>())
                .CreateMapper();
        }

        public static void MapToResultData(QueryResult<UserSummary> result, UserQueryResultResourceData data)
        {
            Mapper.Map(result, data);
        }
    }

    public class UserResultMapperProfile : Profile
    {
        public UserResultMapperProfile()
        {
            CreateMap<QueryResult<UserSummary>, UserQueryResultResourceData>()
                .ForMember(x => x.Items, opts => opts.MapFrom(x => x.Items));
            CreateMap<UserSummary, UserResultResource>()
                .ForMember(x => x.Data, opts => opts.MapFrom(x => x))
                .ForMember(x => x.Links, opts => opts.MapFrom(x => x));
        }
    }
}