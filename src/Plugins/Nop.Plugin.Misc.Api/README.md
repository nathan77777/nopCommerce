# Nop Plugin `Misc.Api` (RestApi)

## Overview

`Nop.Plugin.Misc.Api` is an admin-focused REST plugin for nopCommerce product management.

It provides:
- token-based admin authentication,
- secured product CRUD endpoints,
- product import/export via Excel,
- Swagger/OpenAPI documentation.

From `plugin.json`:
- **Friendly name**: `RestApi`
- **System name**: `Misc.Api`
- **Version**: `1.0`
- **Author**: `Nathan`
- **Description**: `REST API plugin for Product Management`

---

## Main functionalities

### 1) Admin authentication API
Base route: `/api/admin/auth`

- `POST /api/admin/auth/login`
  - Validates nopCommerce credentials (`email` + `password`).
  - Allows access **only** for admin customers.
  - Generates a token (`Guid`) and stores it in cache with key:
    - `Nop.Plugin.Api.Token-{token}`
  - Returns token info:
    - `Token`
    - `ExpiresInMinutes = 120`

- `POST /api/admin/auth/logout`
  - Requires authentication.
  - Removes token from cache.
  - Always returns success message even if token is missing/already invalid.

### 2) Admin product API
Base route: `/api/admin/products`

All endpoints in this controller require `[AdminApiAuthorize]`.

- `GET /api/admin/products`
  - Returns paged product list.
  - Supports extensive search/filter options (category, manufacturer, vendor, store, warehouse, product type, keyword/SKU search, price range, sort, pagination, hidden products, etc.).
  - Response contains:
    - `TotalCount`
    - `TotalPages`
    - `HasNextPage`
    - `Items` (`ProductDto[]`)

- `GET /api/admin/products/{id}`
  - Returns product details (`ProductDetailsDto`).
  - Returns `404` if not found.

- `POST /api/admin/products`
  - Creates a product from `CreateProductDto`.
  - Applies defaults in code:
    - `AdminComment = "Created via REST API"`
    - `ShowOnHomepage = true`
    - `TaxCategoryId = 1`
  - Inserts category mappings from `CategoryIds`.
  - Generates/updates SEO slug through `IUrlRecordService`.

- `PUT /api/admin/products/{id}`
  - Updates an existing product using `CreateProductDto`.
  - Validates model with FluentValidation.
  - Updates category mappings when `CategoryIds` is provided.

- `DELETE /api/admin/products/{id}`
  - Deletes product.
  - Logs activity via `ICustomerActivityService` with action key `DeleteProduct`.

- `POST /api/admin/products/import-excel`
  - Accepts `.xlsx` file upload.
  - Imports products through `IImportManager.ImportProductsFromXlsxAsync`.

- `GET /api/admin/products/export-excel`
  - Exports products to `.xlsx` using `IExportManager.ExportProductsToXlsxAsync`.
  - Optional query params:
    - `categoryId`
    - `manufacturerId` (present but currently not applied in search call)
    - `keyword` (present but currently not applied in search call)
    - `limit` (capped to `5000`)
    - `filename`

### 3) Swagger UI

Configured in plugin startup:

- Swagger JSON:
  - `/swagger/v1-product-admin-api/swagger.json`
- Swagger UI:
  - `/swagger`
  - `/swagger/index.html`

Also includes redirect handling from:
- `/swagger`
- `/swagger/`
- `/swagger/html`

to `/swagger/index.html`.

### 4) Health/debug route

- `GET /api/debug` → returns: `Plugin is alive!`

---

## Security model

Authorization is handled by `AdminApiAuthorizeAttribute`:

1. Reads token from:
   - `Authorization: Bearer <token>` (robustly strips repeated `Bearer ` prefixes and quotes), or
   - `X-Api-Token: <token>` fallback header.
2. Resolves cache key `Nop.Plugin.Api.Token-{token}`.
3. Loads customer by cached ID.
4. Allows request only if customer is:
   - existing,
   - active,
   - not deleted,
   - admin.

If any check fails → `401 Unauthorized`.

---

## Validation rules (`CreateProductDtoValidator`)

- `Name` is required.
- `Price` must be `>= 0`.
- `Sku` is required and must be unique (except for the same product on update).

---

## DTOs exposed by the plugin

- `CreateProductDto`
  - `ProductId`, `Name`, `ShortDescription`, `FullDescription`, `Sku`, `Price`, `Published`, `ProductTypeId`, `CategoryIds`, `ManufacturerIds`
- `ProductDto`
  - `ProductTypeId`, `Name`, `ShortDescription`, `Sku`, `Price`, `Published`
- `ProductDetailsDto` (extends `ProductDto`)
  - `LimitedToStores`, `MetaTitle`, `FullDescription`, `AdminComment`, `MetaDescription`
- `ProductSearchModelDto`
  - search filters + pagination (`PageIndex`, `PageSize`)

---

## Technical notes

- Token lifetime is returned as `120` minutes at login and token is stored in nopCommerce cache.
- Swagger security is configured as `Bearer` in HTTP header.
- The token is a custom GUID token (not a JWT payload).
- The project targets `.NET 10` in the current workspace.

---

## Quick usage flow

1. Install/enable plugin `Misc.Api` in nopCommerce.
2. Call `POST /api/admin/auth/login` with admin credentials.
3. Use returned token in API calls:
   - `Authorization: Bearer <token>`
4. Test endpoints via `/swagger`.
5. Call `POST /api/admin/auth/logout` when done.
