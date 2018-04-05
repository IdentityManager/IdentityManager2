using AutoMapper;
using IdentityManager2.Api.Models;
using IdentityManager2.Core;

namespace IdentityManager2.Mappers
{
    public static class RoleResultMappers
    {
        internal static IMapper Mapper { get; }

        static RoleResultMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<RoleResultMapperProfile>())
                .CreateMapper();
        }

        public static void MapToResultData(QueryResult<RoleSummary> result, RoleQueryResultResourceData data)
        {
            Mapper.Map(result, data);
        }
    }

    public class RoleResultMapperProfile : Profile
    {
        public RoleResultMapperProfile()
        {
            CreateMap<QueryResult<RoleSummary>, RoleQueryResultResourceData>()
                .ForMember(x => x.Items, opts => opts.MapFrom(x => x.Items));
            CreateMap<RoleSummary, RoleResultResource>()
                .ForMember(x => x.Data, opts => opts.MapFrom(x => x))
                .ForMember(x => x.Links, opts => opts.MapFrom(x => x));
        }
    }
}