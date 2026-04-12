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
using Product = Nop.Core.Domain.Catalog.Product;

namespace Nop.Plugin.Misc.Api.Controllers;

/// <summary>
/// Manages products in the admin API.
/// </summary>
[AdminApiAuthorize]
[ApiController]
[Route("api/admin/products")]
[Produces("application/json")]
public class ProductApiAdminController : ControllerBase
{
    #region Properties

    /// <summary>
    /// NopCommerce product service for managing products, including CRUD operations and product searches.
    /// </summary>
    protected readonly IProductService _productService;
    /// <summary>
    /// NopCommerce URL record service for managing SEO-friendly URLs and slugs for products.
    /// </summary>
    protected readonly IUrlRecordService _urlRecordService;
    /// <summary>
    /// NopCommerce category service for managing product-category relationships and retrieving category information.
    /// </summary>
    protected readonly ICategoryService _categoryService;
    /// <summary>
    /// NopCommerce FluentValidation validator for validating product creation and update models.
    /// </summary>
    protected readonly IValidator<CreateProductDto> _validator;
    /// <summary>
    /// NopCommerce customer activity service for logging admin actions related to products.
    /// </summary>
    protected readonly ICustomerActivityService _customerActivityService;
    /// <summary>
    /// NopCommerce import manager for handling product imports from Excel files.
    /// </summary>
    protected readonly IImportManager _importManager;
    /// <summary>
    /// NopCommerce export manager for handling product exports to Excel files.
    /// </summary>
    protected readonly IExportManager _exportManager;

    /// <summary>
    /// Maximum number of products that can be exported at once.
    /// </summary>
    protected int _maxExportLimit = 5000;     

    #endregion

    #region Ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductApiAdminController"/> class.
    /// </summary>
    /// <param name="productService">The product service</param>
    /// <param name="categoryService">The category service</param>
    /// <param name="urlRecordService">The URL record service</param>
    /// <param name="validator">The product validator</param>
    /// <param name="customerActivityService">The customer activity service</param>
    /// <param name="importManager">The import manager</param>
    /// <param name="exportManager">The export manager</param>
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

    /// <summary>
    /// Gets a list of all products based on the search criteria.
    /// </summary>
    /// <param name="model">Search model</param>
    /// <returns>A list of products</returns>
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

    /// <summary>
    /// Gets the details of a specific product by its identifier.
    /// </summary>
    /// <param name="id">Product identifier</param>
    /// <returns>Product details</returns>
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


    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="model">Product creation model</param>
    /// <returns>Created product</returns>
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


    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="id">Product identifier</param>
    /// <param name="model">Product update model</param>
    /// <returns>Updated product</returns>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProductAsync(int id, [FromBody] CreateProductDto model)
    {
        model.ProductId ??= id;
        var validationResult = await _validator.ValidateAsync(model);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var existingProduct = await _productService.GetProductByIdAsync(model.ProductId.Value);
        if (existingProduct == null)
            return NotFound("Product not found with id: " + model.ProductId);

        // Map updated fields onto the existing tracked entity to preserve its ID and audit fields
        AutoMapperConfiguration.Mapper.Map(model, existingProduct);
        await _productService.UpdateProductAsync(existingProduct);

        // Sync category relationships: delete all existing, then re-insert from the request
        if (model.CategoryIds.Count > 0)
        {
            var existingCategories = await _categoryService.GetProductCategoriesByProductIdAsync(existingProduct.Id, showHidden: true);

            foreach (var existingCategory in existingCategories)
                await _categoryService.DeleteProductCategoryAsync(existingCategory);

            foreach (var categoryId in model.CategoryIds)
            {
                await _categoryService.InsertProductCategoryAsync(new ProductCategory
                {
                    ProductId = existingProduct.Id,
                    CategoryId = categoryId,
                    IsFeaturedProduct = false,
                    DisplayOrder = 1
                });
            }
        }

        // Refresh SEO slug in case the product name changed
        await _urlRecordService.SaveSlugAsync(existingProduct,
            await _urlRecordService.GetSeNameAsync(existingProduct.Id, existingProduct.Name), 0);

        return Ok(model);
    }

    /// <summary>
    /// Deletes a product by its identifier.
    /// </summary>
    /// <param name="id">Product identifier</param>
    /// <returns>No content on success</returns>
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


    /// <summary>
    /// Imports products from an Excel file.
    /// </summary>
    /// <param name="file">Excel file containing product data</param>
    /// <returns>Status of the import operation</returns>
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


    /// <summary>
    /// Exports products to an Excel file.
    /// </summary>
    /// <param name="categoryId">Category identifier to filter by</param>
    /// <param name="manufacturerId">Manufacturer identifier to filter by</param>
    /// <param name="keyword">Keyword to search for</param>
    /// <param name="limit">Maximum number of records to export</param>
    /// <param name="filename">Name of the exported file</param>
    /// <returns>Excel file</returns>
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
