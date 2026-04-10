using DocumentFormat.OpenXml.Math;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Misc.Api.DTO;
using Nop.Plugin.Misc.Api.Filters;
using Nop.Services.Catalog;
using Nop.Services.ExportImport;
using Nop.Services.Logging;
using Nop.Services.Seo;
using Nop.Web.Framework.Controllers;
using Product = Nop.Core.Domain.Catalog.Product;

namespace Nop.Plugin.Misc.Api.Controllers;

[AdminApiAuthorize]
[ApiController]
[Route("api/admin/products")]
public class ProductApiAdminController : BasePluginController
{
    #region Properties

    protected readonly IProductService _productService;
    protected readonly IUrlRecordService _urlRecordService;
    protected readonly ICategoryService _categoryService;
    protected readonly IValidator<CreateProductDto> _validator;
    protected readonly ICustomerActivityService _customerActivityService;
    protected readonly IImportManager _importManager;
    protected readonly IExportManager _exportManager;

    protected int _maxExportLimit = 5000;     

    #endregion

    #region Ctor

    public ProductApiAdminController(
        IProductService productService,
        ICategoryService categoryService,
        IUrlRecordService urlRecordService,
        IValidator<CreateProductDto> validator,
        ICustomerActivityService customerActivityService,
        IImportManager importManager,
        IExportManager exportManager)
    {
        _productService = productService;
        _categoryService = categoryService;
        _urlRecordService = urlRecordService;
        _validator = validator;
        _customerActivityService = customerActivityService;
        _importManager = importManager;
        _exportManager = exportManager;
    }

    #endregion

    #region Methods

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProductsAsync(
        [FromQuery] ProductSearchModelDto model)
    {
        try
        {
            var products = await _productService.SearchProductsAsync(
                pageIndex: model.PageIndex,
                pageSize: model.PageSize,
                categoryIds: model.CategoryIds,
                manufacturerIds: model.ManufacturerIds,
                storeId: model.StoreId,
                vendorId: model.VendorId,
                warehouseId: model.WarehouseId,
                productType: model.ProductType,
                visibleIndividuallyOnly: model.VisibleIndividuallyOnly,
                excludeFeaturedProducts: model.ExcludeFeaturedProducts,
                priceMin: model.PriceMin,
                priceMax: model.PriceMax,
                productTagId: model.ProductTagId,
                keywords: model.Keywords,
                searchDescriptions: model.SearchDescriptions,
                searchManufacturerPartNumber: model.SearchManufacturerPartNumber,
                searchSku: model.SearchSku,
                searchProductTags: model.SearchProductTags,
                languageId: model.LanguageId,
                filteredSpecOptions: model.FilteredSpecOptions,
                orderBy: model.OrderBy,
                showHidden: model.ShowHidden,
                overridePublished: model.OverridePublished
            );

            var res = products.Select(p => AutoMapperConfiguration.Mapper.Map<ProductDto>(p)).ToList();

            return Ok(new
            {
                TotalCount = products.TotalCount,
                TotalPages = products.TotalPages,
                HasNextPage = products.HasNextPage,
                Items = res
            });
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }

    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetProductDetailsAsync(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
        {
            return NotFound("Product not found with id: " + id);
        }

        var res = AutoMapperConfiguration.Mapper.Map<ProductDetailsDto>(product);
        return Ok((res));
    }


    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateProductDto model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var product = AutoMapperConfiguration.Mapper.Map<Product>(model);

        // Default values
        product.AdminComment = "Created via REST API";
        product.ShowOnHomepage = true;
        product.TaxCategoryId = 1;

        // 3. Insertion du produit (C'est ici qu'il récupère son ID)
        await _productService.InsertProductAsync(product);

        // 4. Liaison des données (Relations Many-to-Many)
        if (model.CategoryIds.Count != 0)
        {
            foreach (var categoryId in model.CategoryIds)
            {
                await _categoryService.InsertProductCategoryAsync(new ProductCategory
                {
                    ProductId = product.Id,
                    CategoryId = categoryId,
                    IsFeaturedProduct = false,
                    DisplayOrder = 1
                });
            }
        }

        // 5. Mise à jour des URLs (SEO)
        await _urlRecordService.SaveSlugAsync(product,
            await _urlRecordService.GetSeNameAsync(product.Id, product.Name), 0);

        return CreatedAtAction("GetProductDetails", new { id = product.Id }, product.Id);
    }


    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProductAsync(int id, [FromBody] CreateProductDto model)
    {
        model.ProductId ??= id;
        var validationResult = await _validator.ValidateAsync(model);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var oldProduct = await _productService.GetProductByIdAsync(model.ProductId.Value);
        if (oldProduct == null) return NotFound("Product not found with id: " + model.ProductId);

        var newProduct = AutoMapperConfiguration.Mapper.Map<Product>(model);
        await _productService.UpdateProductAsync(newProduct);

        if (model.CategoryIds.Count == 0) return Ok(model);

        foreach (var categoryId in model.CategoryIds)
        {
            await _categoryService.UpdateProductCategoryAsync(new ProductCategory
            {
                ProductId = newProduct.Id,
                CategoryId = categoryId,
                IsFeaturedProduct = false,
                DisplayOrder = 1
            });
        }

        return Ok(model);

    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProductAsync(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
            return NotFound();

        await _productService.DeleteProductAsync(product);

        // Log who did the action
        await _customerActivityService.InsertActivityAsync("DeleteProduct", $"Deleted product: {product.Name}", product);

        return NoContent();
    }


    [HttpPost("import-excel")]
    public async Task<IActionResult> ImportProductsFromExcelAsync(IFormFile file)
    {
        Console.WriteLine("File received: " + file?.FileName);
        if (file == null || !file.FileName.EndsWith(".xlsx"))
            return BadRequest("Please provide a valid Excel file.");
        
        try
        {
            await using (var stream = file.OpenReadStream())
            {
                await _importManager.ImportProductsFromXlsxAsync(stream);
            }
            return Ok("Products successfully loaded");
        }
        catch (Exception e)
        {
            return StatusCode(500, $"Error while importing file: {e.Message}");
        }
    }


    [HttpGet("export-excel")]
    public async Task<IActionResult> ExportProductsToExcelAsync(
        int categoryId = 0,
        int manufacturerId = 0,
        string keyword = null,
        int limit = 100,
        string filename = null)
    {
        var finalLimit = Math.Min(limit, _maxExportLimit);
        var products = await _productService.SearchProductsAsync(
            categoryIds: categoryId > 0 ? new List<int> { categoryId } : null,
            pageIndex: 0,
            pageSize: finalLimit,
            showHidden: true
        );

        if (!products.Any())
            return NotFound("No product found for this search");

        var bytes = await _exportManager.ExportProductsToXlsxAsync(products);
        var exportFilename = string.IsNullOrEmpty(filename) ? "export.xlsx" : filename + ".xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", exportFilename);
    }

    #endregion

}
