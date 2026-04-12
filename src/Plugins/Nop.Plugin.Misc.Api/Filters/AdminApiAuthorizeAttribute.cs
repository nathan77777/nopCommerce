using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core.Caching;
using Nop.Plugin.Misc.Api.Controllers;
using Nop.Services.Customers;

namespace Nop.Plugin.Misc.Api.Filters;

/// <summary>
/// Represents a filter attribute that confirms access to the admin api
/// </summary>
public class AdminApiAuthorizeAttribute : TypeFilterAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AdminApiAuthorizeAttribute"/> class.
    /// </summary>
    public AdminApiAuthorizeAttribute() : base(typeof(AdminApiAuthorizeFilter))
    {
    }

    private class AdminApiAuthorizeFilter : IAsyncAuthorizationFilter
    {

        private readonly IStaticCacheManager _staticCacheManager;
        private readonly ICustomerService _customerService;

        public AdminApiAuthorizeFilter(IStaticCacheManager staticCacheManager, ICustomerService customerService)
        {
            _staticCacheManager = staticCacheManager;
            _customerService = customerService;
        }

        // AdminApiAuthorizeFilter - OnAuthorizationAsync
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var token = AuthController.ExtractToken(context.HttpContext);

            if (string.IsNullOrWhiteSpace(token))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var cacheKey = new CacheKey($"Nop.Plugin.Api.Token-{token}");
            var customerId = await _staticCacheManager.GetAsync<int?>(cacheKey, () => Task.FromResult<int?>(null));

            if (!customerId.HasValue)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var customer = await _customerService.GetCustomerByIdAsync(customerId.Value);
            if (customer is not { Active: true } || customer.Deleted || !await _customerService.IsAdminAsync(customer)) 
                context.Result = new UnauthorizedResult();
        }

    }
}