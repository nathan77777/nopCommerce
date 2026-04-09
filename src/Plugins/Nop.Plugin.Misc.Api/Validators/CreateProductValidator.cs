using FluentValidation;
using Nop.Plugin.Misc.Api.DTO;
using Nop.Services.Catalog;
using Nop.Services.Localization;

namespace Nop.Plugin.Misc.Api.Validators;

public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    protected readonly IProductService _productService;

    public CreateProductDtoValidator(ILocalizationService localizationService, IProductService productService)
    {
        _productService = productService;

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("The name is mandatory.");
        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0)
            .WithMessage("The price cannot be negative.");
        RuleFor(x => x.Sku)
            .NotEmpty()
            .WithMessage("SKU is required for inventory.")
            .MustAsync(async (dto, sku, cancellation) =>
            {
                var existingProduct = await _productService.GetProductBySkuAsync(sku);
                Console.WriteLine($"Checking SKU: {sku}, Existing Product ID: {existingProduct?.Id}, DTO Product ID: {dto.ProductId}");
                return existingProduct == null || existingProduct.Id == dto.ProductId;
            })
            .WithMessage("Sku already used by another product.");
    }
}
