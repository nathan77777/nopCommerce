using System;
using System.Collections.Generic;
using System.Text;
using Nop.Core.Domain.Catalog;

namespace Nop.Plugin.Misc.Api.DTO;
public class ProductSearchModelDto
{
    public IList<int> CategoryIds { get; set; }  = null;
    public IList<int> ManufacturerIds { get; set; } = null;
    public int StoreId { get; set; } = 0;
    public int VendorId { get; set; } = 0;
    public int WarehouseId { get; set; } = 0;
    public ProductType? ProductType { get; set; } = null;
    public bool VisibleIndividuallyOnly { get; set; } = false;
    public bool ExcludeFeaturedProducts { get; set; } = false;
    public decimal? PriceMin { get; set; } = null;
    public decimal? PriceMax { get; set; } = null;
    public int ProductTagId { get; set; } = 0;
    public string Keywords { get; set; } = null;
    public bool SearchDescriptions { get; set; } = false;
    public bool SearchManufacturerPartNumber { get; set; } = true;
    public bool SearchSku { get; set; } = true;
    public bool SearchProductTags { get; set; } = false;
    public int LanguageId { get; set; } = 0;
    public IList<SpecificationAttributeOption> FilteredSpecOptions { get; set; } = null;
    public ProductSortingEnum OrderBy { get; set; } = ProductSortingEnum.Position;
    public bool ShowHidden { get; set; } = false;
    public bool? OverridePublished { get; set; } = null;

    // Pagination (Indispensable !)
    public int PageIndex { get; set; } = 0;
    public int PageSize { get; set; } = 20;
}
