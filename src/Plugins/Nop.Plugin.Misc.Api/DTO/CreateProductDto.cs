using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace Nop.Plugin.Misc.Api.DTO;

public class CreateProductDto
{
    public int? ProductId { get; set; }
    public string Name { get; set; }
    public string ShortDescription { get; set; }
    public string FullDescription { get; set; }
    public string Sku { get; set; }
    public decimal Price { get; set; } = 0;
    public bool Published { get; set; }
    public int ProductTypeId { get; set; } = 5;
    public List<int> CategoryIds { get; set; } = [];
    public List<int> ManufacturerIds { get; set; } = [];
}
