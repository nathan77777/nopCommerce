
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Misc.Api.DTO;
using Nop.Services.Catalog;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Misc.Api.Controllers;

[ApiController]
[Route("api/admin/products")]
public class ProductApiAdminController : BasePluginController
{
    protected readonly IProductService _productService;

    public ProductApiAdminController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProductsAsync()
    {
        try
        {
            var products = await _productService.GetAllProductsDisplayedOnHomepageAsync();
            var res = products.Select(p => AutoMapperConfiguration.Mapper.Map<ProductDto>(p)).ToList();
            return Ok(res);
        }
        catch (Exception e)
        {
            return Problem(e.Message);
        }

    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDetailsDto>> GetProductDetailsAsync(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
        {
            return NotFound("Product not found with id: " + id);
        }

        var res = AutoMapperConfiguration.Mapper.Map<ProductDetailsDto>(product);
        return Ok((res));
    }

    

}
