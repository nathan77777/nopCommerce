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
    public async Task<IActionResult> GetRecentlyViewedProductsAsync()
    {
        Console.WriteLine(">>> GetRecentlyViewedProductsAsync called <<<");
        return Ok("GetRecentlyViewedProductsAsync called");
    }
}