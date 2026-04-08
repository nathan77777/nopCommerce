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

        RuleFor(x => x.Name).NotEmpty().WithMessage("The name is mandatory.");
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).WithMessage("The price cannot be negative.");
        RuleFor(x => x.Sku).NotEmpty().WithMessage("SKU is required for inventory.");
        RuleFor(x => x).MustAsync(async (x, cancellation) =>
        {
            var existingProduct = await _productService.GetProductBySkuAsync(x.Sku);
            return existingProduct == null || existingProduct.Id == x.ProductId;
        }).WithMessage("Sku already used by another product.");
    }
}
