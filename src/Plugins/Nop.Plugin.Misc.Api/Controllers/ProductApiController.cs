namespace Nop.Plugin.Misc.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Seo;
using Nop.Web.Framework.Controllers;

[ApiController]
[Route("api/products")]
public class ProductApiController : BasePluginController
{
    protected readonly IProductService _productService;
    protected readonly IUrlRecordService _urlRecordService;
    protected readonly CatalogSettings _catalogSettings;
    protected readonly IRecentlyViewedProductsService _recentlyViewedProductsService;

    public ProductApiController(
        IProductService productService,
        IUrlRecordService urlRecordService,
        CatalogSettings catalogSettings,
        IRecentlyViewedProductsService recentlyViewedProductsService)
    {
        _productService = productService;
        _urlRecordService = urlRecordService;
        _catalogSettings = catalogSettings;
        _recentlyViewedProductsService = recentlyViewedProductsService;
    }

    [HttpGet]  // → GET api/products
    public async Task<ActionResult<IEnumerable<Product>>> GetRecentlyViewedProductsAsync()
    {
        if (!_catalogSettings.RecentlyViewedProductsEnabled)
            return Content("");

        var products = await _recentlyViewedProductsService.GetRecentlyViewedProductsAsync(_catalogSettings.RecentlyViewedProductsNumber);
        return Ok(products);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetProductBySlugAsync(string slug)
    {
        if (slug == null)
        {
            return BadRequest("Product slug cannot be null.");
        }

        var element = await _urlRecordService.GetBySlugAsync(slug);
        if (element == null)
        {
            return BadRequest("Unknown product");
        }

        if (!element.IsActive || !element.EntityName.Equals(nameof(Product)))
        {
            return BadRequest("The element is either inactive or not a product");
        }

        var product = _productService.GetProductByIdAsync(element.EntityId);
        return Ok(product);


    }
}