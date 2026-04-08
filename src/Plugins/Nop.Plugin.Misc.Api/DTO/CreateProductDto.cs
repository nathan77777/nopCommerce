using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.Api.DTO;

public class CreateProductDto
{
    public string Name { get; set; }
    public string ShortDescription { get; set; }
    public string FullDescription { get; set; }
    public string Sku { get; set; }
    public decimal Price { get; set; }
    public bool Published { get; set; }
    public int ProductTypeId { get; set; } = 5;
}
