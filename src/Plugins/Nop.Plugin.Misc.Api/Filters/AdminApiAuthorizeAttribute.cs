using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
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
            var rawHeader = context.HttpContext.Request.Headers.Authorization.ToString();
            Console.WriteLine($">>> [AdminApiAuthorize] Raw Authorization header: '{rawHeader}'");

            var token = AuthController.ExtractToken(context.HttpContext);
            Console.WriteLine($">>> [AdminApiAuthorize] Extracted token: '{token}'");

            if (string.IsNullOrWhiteSpace(token))
            {
                Console.WriteLine(">>> [AdminApiAuthorize] Token is null/empty → 401");
                context.Result = new UnauthorizedResult();
                return;
            }

            var cacheKey = new CacheKey($"Nop.Plugin.Api.Token-{token}");
            var customerId = await _staticCacheManager.GetAsync<int?>(cacheKey, () => Task.FromResult<int?>(null));
            Console.WriteLine($">>> [AdminApiAuthorize] CustomerId from cache: '{customerId}'");

            if (!customerId.HasValue)
            {
                Console.WriteLine(">>> [AdminApiAuthorize] CustomerId not found in cache → 401");
                context.Result = new UnauthorizedResult();
                return;
            }

            var customer = await _customerService.GetCustomerByIdAsync(customerId.Value);
            Console.WriteLine($">>> [AdminApiAuthorize] Customer active: {customer?.Active}, Deleted: {customer?.Deleted}");

            if (customer is not { Active: true } || customer.Deleted || !await _customerService.IsAdminAsync(customer))
            {
                Console.WriteLine(">>> [AdminApiAuthorize] Customer invalid or not admin → 401");
                context.Result = new UnauthorizedResult();
            }
        }

        private static string ExtractToken(HttpContext httpContext)
        {
            const string bearerPrefix = "Bearer ";

            var authHeader = httpContext.Request.Headers.Authorization.ToString().Trim();

            // Strip tous les préfixes "Bearer " (cas Swagger double-préfixe)
            while (authHeader.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
                authHeader = authHeader[bearerPrefix.Length..].Trim();

            // Retire les guillemets éventuels
            authHeader = authHeader.Trim('"');

            // Fallback sur X-Api-Token
            if (string.IsNullOrWhiteSpace(authHeader))
                authHeader = httpContext.Request.Headers["X-Api-Token"].ToString().Trim().Trim('"');

            return authHeader;
        }
    }
}