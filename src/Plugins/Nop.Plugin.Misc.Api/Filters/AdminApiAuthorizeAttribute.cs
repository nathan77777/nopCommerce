using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core.Caching;
using Nop.Services.Customers;

namespace Nop.Plugin.Misc.Api.Filters;

/// <summary>
/// Represents a filter attribute that confirms access to the admin api
/// </summary>
public class AdminApiAuthorizeAttribute : TypeFilterAttribute
{
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

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var token = context.HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var cacheKey = new CacheKey($"Nop.Plugin.Api.Token-{token}");
            var customerId = await _staticCacheManager.GetAsync<int?>(cacheKey);

            if (!customerId.HasValue)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Re-verify in DB if the account has not been deactivated in the meantime
            var customer = await _customerService.GetCustomerByIdAsync(customerId.Value);
            if (customer is not { Active: true } || customer.Deleted || !await _customerService.IsAdminAsync(customer)) 
                context.Result = new UnauthorizedResult();
        }
    }
}