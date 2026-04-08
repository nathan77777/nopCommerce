using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.Api.DTO;

public class ProductDetailsDto : ProductDto
{
    /// <summary>
    /// Gets or sets a value indicating whether the entity is limited/restricted to certain stores
    /// </summary>
    public bool LimitedToStores { get; set; }

    /// <summary>
    /// Gets or sets the meta title
    /// </summary>
    public string MetaTitle { get; set; }

    /// <summary>
    /// Gets or sets the full description
    /// </summary>
    public string FullDescription { get; set; }

    /// <summary>
    /// Gets or sets the admin comment
    /// </summary>
    public string AdminComment { get; set; }

    /// <summary>
    /// Gets or sets the meta description
    /// </summary>
    public string MetaDescription { get; set; }
}
