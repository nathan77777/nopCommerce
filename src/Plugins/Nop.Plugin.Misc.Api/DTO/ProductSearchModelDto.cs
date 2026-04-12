using System;
using System.Collections.Generic;
using System.Text;
using Nop.Core.Domain.Catalog;

namespace Nop.Plugin.Misc.Api.DTO;

/// <summary>
/// Represents the search criteria for querying products via the admin API.
/// All parameters are optional and default to their NopCommerce equivalents.
/// </summary>
public class ProductSearchModelDto
{
    /// <summary>
    /// Gets or sets the list of category identifiers to filter products by.
    /// Returns products belonging to any of the specified categories.
    /// Defaults to <c>null</c> (no category filter).
    /// </summary>
    public IList<int> CategoryIds { get; set; } = null;

    /// <summary>
    /// Gets or sets the list of manufacturer identifiers to filter products by.
    /// Returns products associated with any of the specified manufacturers.
    /// Defaults to <c>null</c> (no manufacturer filter).
    /// </summary>
    public IList<int> ManufacturerIds { get; set; } = null;

    /// <summary>
    /// Gets or sets the store identifier to filter products by.
    /// Use <c>0</c> to include products from all stores.
    /// </summary>
    public int StoreId { get; set; } = 0;

    /// <summary>
    /// Gets or sets the vendor identifier to filter products by.
    /// Use <c>0</c> to include products from all vendors.
    /// </summary>
    public int VendorId { get; set; } = 0;

    /// <summary>
    /// Gets or sets the warehouse identifier to filter products by.
    /// Use <c>0</c> to include products from all warehouses.
    /// </summary>
    public int WarehouseId { get; set; } = 0;

    /// <summary>
    /// Gets or sets the product type to filter by (e.g. Simple, Grouped).
    /// Defaults to <c>null</c> (no product type filter).
    /// </summary>
    public ProductType? ProductType { get; set; } = null;

    /// <summary>
    /// Gets or sets a value indicating whether to return only products
    /// that are visible individually (i.e. not only shown as part of a grouped product).
    /// </summary>
    public bool VisibleIndividuallyOnly { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to exclude featured products from the results.
    /// </summary>
    public bool ExcludeFeaturedProducts { get; set; } = false;

    /// <summary>
    /// Gets or sets the minimum price to filter products by.
    /// Defaults to <c>null</c> (no lower price bound).
    /// </summary>
    public decimal? PriceMin { get; set; } = null;

    /// <summary>
    /// Gets or sets the maximum price to filter products by.
    /// Defaults to <c>null</c> (no upper price bound).
    /// </summary>
    public decimal? PriceMax { get; set; } = null;

    /// <summary>
    /// Gets or sets the product tag identifier to filter products by.
    /// Use <c>0</c> to ignore product tags.
    /// </summary>
    public int ProductTagId { get; set; } = 0;

    /// <summary>
    /// Gets or sets the keyword to search for in product names (and optionally descriptions, SKU, tags).
    /// Defaults to <c>null</c> (no keyword filter).
    /// </summary>
    public string Keywords { get; set; } = null;

    /// <summary>
    /// Gets or sets a value indicating whether to include product descriptions in the keyword search.
    /// </summary>
    public bool SearchDescriptions { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to include the manufacturer part number in the keyword search.
    /// </summary>
    public bool SearchManufacturerPartNumber { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to include the SKU field in the keyword search.
    /// </summary>
    public bool SearchSku { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to include product tags in the keyword search.
    /// </summary>
    public bool SearchProductTags { get; set; } = false;

    /// <summary>
    /// Gets or sets the language identifier used to search localized product fields.
    /// Use <c>0</c> for the default language.
    /// </summary>
    public int LanguageId { get; set; } = 0;

    /// <summary>
    /// Gets or sets the list of specification attribute options to filter products by.
    /// Defaults to <c>null</c> (no specification filter).
    /// </summary>
    public IList<SpecificationAttributeOption> FilteredSpecOptions { get; set; } = null;

    /// <summary>
    /// Gets or sets the sort order to apply to the results.
    /// Defaults to <see cref="ProductSortingEnum.Position"/>.
    /// </summary>
    public ProductSortingEnum OrderBy { get; set; } = ProductSortingEnum.Position;

    /// <summary>
    /// Gets or sets a value indicating whether to include hidden (unpublished) products in the results.
    /// Should only be <c>true</c> for admin contexts.
    /// </summary>
    public bool ShowHidden { get; set; } = false;

    /// <summary>
    /// Gets or sets an override for the published filter.
    /// Use <c>true</c> to return only published products, <c>false</c> for unpublished only,
    /// or <c>null</c> to apply no override (respects <see cref="ShowHidden"/>).
    /// </summary>
    public bool? OverridePublished { get; set; } = null;

    /// <summary>
    /// Gets or sets the zero-based page index for pagination.
    /// Defaults to <c>0</c> (first page).
    /// </summary>
    public int PageIndex { get; set; } = 0;

    /// <summary>
    /// Gets or sets the number of products to return per page.
    /// Defaults to <c>20</c>.
    /// </summary>
    public int PageSize { get; set; } = 20;
}