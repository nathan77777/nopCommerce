using AutoMapper;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Misc.Api.DTO;

namespace Nop.Plugin.Misc.Api.Infrastructure;

/// <summary>
/// Configures AutoMapper mappings for the plugin.
/// </summary>
public class AutoMapperConfiguration : Profile, IOrderedMapperProfile
{

    #region Ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="AutoMapperConfiguration"/> class.
    /// </summary>
    public AutoMapperConfiguration()
    {
        CreateMap<Product, ProductDto>();
        CreateMap<Product, ProductDetailsDto>();
        CreateMap<CreateProductDto, Product>();
    }

    #endregion


    /// <summary>
    /// Order of this mapper implementation
    /// </summary>
    public int Order => 100;
}
