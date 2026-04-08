
using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.Api.DTO;
using Nop.Services.Catalog;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Misc.Api.Controllers;

[ApiController]
[Route("api/admin/products")]
public class ProductApiAdminController : BasePluginController
{
    protected readonly IProductService _productService;
    protected readonly IMapper _mapper;

    public ProductApiAdminController(IProductService productService, IMapper mapper)
    {
        _productService = productService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProductsAsync()
    {
        try
        {
            var products = await _productService.GetAllProductsDisplayedOnHomepageAsync();
            var res = products.Select(product => _mapper.Map<ProductDto>(product)).ToList();
            return Ok(res);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Problem(e.Message);
        }

    }
}
