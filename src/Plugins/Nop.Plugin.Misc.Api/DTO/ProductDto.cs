namespace Nop.Plugin.Misc.Api.DTO;

public class ProductDto
{
    /// <summary>
    /// Gets or sets the product type identifier
    /// </summary>
    public int ProductTypeId { get; set; }
    /// <summary>
    /// Gets or sets the name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the short description
    /// </summary>
    public string ShortDescription { get; set; }

    /// <summary>
    /// Gets or sets the SKU
    /// </summary>
    public string Sku { get; set; }

    /// <summary>
    /// Gets or sets the price
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the entity is published
    /// </summary>
    public bool Published { get; set; }
}