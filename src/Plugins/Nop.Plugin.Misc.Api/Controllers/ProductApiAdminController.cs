using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Misc.Api.DTO;
using Nop.Services.Catalog;
using Nop.Services.Logging;
using Nop.Services.Seo;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Misc.Api.Controllers;

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

    #endregion

    #region Ctor

    public ProductApiAdminController(
        IProductService productService,
        ICategoryService categoryService,
        IUrlRecordService urlRecordService,
        IValidator<CreateProductDto> validator,
        ICustomerActivityService customerActivityService)
    {
        _productService = productService;
        _categoryService = categoryService;
        _urlRecordService = urlRecordService;
        _validator = validator;
        _customerActivityService = customerActivityService;
    }

    #endregion

    #region Methods

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

        product.Deleted = true;
        product.Published = false;
        product.UpdatedOnUtc = DateTime.UtcNow;

        await _productService.UpdateProductAsync(product);

        // Log who did the action
        await _customerActivityService.InsertActivityAsync("DeleteProduct", $"Deleted product: {product.Name}", product);

        return NoContent();
    }

    #endregion

}
