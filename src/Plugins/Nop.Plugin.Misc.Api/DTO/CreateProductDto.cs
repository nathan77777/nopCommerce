using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace Nop.Plugin.Misc.Api.DTO;

/// <summary>
/// Represents a data transfer object for creating and updating products.
/// </summary>
public class CreateProductDto
{
    /// <summary>
    /// Gets or sets the product identifier.
    /// </summary>
    public int? ProductId { get; set; }
    
    /// <summary>
    /// Gets or sets the name of the product.
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Gets or sets the short description of the product.
    /// </summary>
    public string ShortDescription { get; set; }
    
    /// <summary>
    /// Gets or sets the full description of the product.
    /// </summary>
    public string FullDescription { get; set; }
    
    /// <summary>
    /// Gets or sets the SKU.
    /// </summary>
    public string Sku { get; set; }
    
    /// <summary>
    /// Gets or sets the price of the product.
    /// </summary>
    public decimal Price { get; set; } = 0;
    
    /// <summary>
    /// Gets or sets a value indicating whether the product is published.
    /// </summary>
    public bool Published { get; set; }
    
    /// <summary>
    /// Gets or sets the product type identifier.
    /// </summary>
    public int ProductTypeId { get; set; } = 5;
    
    /// <summary>
    /// Gets or sets the category identifiers mapped to the product.
    /// </summary>
    public List<int> CategoryIds { get; set; } = [];
    
    /// <summary>
    /// Gets or sets the manufacturer identifiers mapped to the product.
    /// </summary>
    public List<int> ManufacturerIds { get; set; } = [];
}
