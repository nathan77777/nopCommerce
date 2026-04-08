using AutoMapper;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Misc.Api.DTO;

namespace Nop.Plugin.Misc.Api.Infrastructure;

public class AutoMapperConfiguration : Profile, IOrderedMapperProfile
{

    #region Ctor

    public AutoMapperConfiguration()
    {
        CreateMap<Product, ProductDto>();
        CreateMap<Product, ProductDetailsDto>();
    }

    #endregion


    /// <summary>
    /// Order of this mapper implementation
    /// </summary>
    public int Order => 100;
}
