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
    /// Gets or sets the old price
    /// </summary>
    public decimal OldPrice { get; set; }

    /// <summary>
    /// Gets or sets the product cost
    /// </summary>
    public decimal ProductCost { get; set; }
}