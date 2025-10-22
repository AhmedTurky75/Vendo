# Vendo Tenant Management Service

## Overview

The Tenant Management Service is responsible for managing stores (tenants) in the Vendo multi-tenant e-commerce platform. It provides APIs for creating, updating, and retrieving store information, including merchant details, store settings, and subscription management.

## Architecture

This service follows **Clean Architecture** principles with the following layers:

- **Domain**: Core business entities, value objects, and domain interfaces
- **Application**: CQRS commands/queries, DTOs, validators, and business logic
- **Infrastructure**: Data access with Entity Framework Core, repositories
- **Api**: REST API controllers, middleware, and configuration

## Technology Stack

- **.NET 9.0**
- **ASP.NET Core Web API**
- **Entity Framework Core 9.0** (SQL Server)
- **MediatR** (CQRS pattern)
- **FluentValidation** (Request validation)
- **Serilog** (Structured logging)
- **Swagger/OpenAPI** (API documentation)

## Domain Model

### Entities

#### Store (Aggregate Root)
Represents a merchant's store in the multi-tenant system.

**Properties:**
- `Id` (Guid): Unique identifier
- `Name` (string): Store display name
- `Subdomain` (Subdomain): Unique URL-safe subdomain
- `MerchantInfo` (MerchantInfo): Merchant contact information
- `Settings` (StoreSettings): Store configuration settings
- `Status` (StoreStatus): Operational status (Active, Inactive, Suspended, Archived)
- `SubscriptionTier` (SubscriptionTier): Free, Starter, or Pro
- `OwnerId` (string): Reference to the merchant user
- `TrialEndsAt` (DateTime?): Trial period end date
- `IsInTrial` (bool): Computed property indicating trial status

### Value Objects

#### Subdomain
Encapsulates subdomain validation logic:
- 3-63 characters
- Lowercase letters, numbers, and hyphens only
- Must start and end with alphanumeric character
- Reserved subdomains are blocked (admin, api, www, etc.)

#### MerchantInfo
Contains merchant contact and business information:
- Email (required)
- Phone, BusinessName, Address, City, State, PostalCode, Country (optional)

#### StoreSettings
Store configuration and branding:
- Currency (ISO 4217, default: USD)
- Timezone (IANA, default: America/New_York)
- Language (ISO 639-1, default: en)
- TaxRate (0-100%, default: 0)
- TaxEnabled (bool)
- PrimaryColor, AccentColor (hex color codes)
- LogoUrl (URL to uploaded logo)

### Enumerations

#### StoreStatus
- Active: Store is operational
- Inactive: Temporarily inactive
- Suspended: Suspended due to policy violation
- Archived: No longer operational

#### SubscriptionTier
- Free: 50 products, 100 orders/month, 1 admin user
- Starter ($29/mo): 500 products, 1000 orders/month, 3 admin users
- Pro ($79/mo): Unlimited products/orders, 10 admin users

## Application Layer (CQRS)

### Commands

#### CreateStoreCommand
Creates a new store with merchant information.

**Endpoint**: `POST /api/stores`

**Request Body**:
```json
{
  "name": "My Awesome Store",
  "subdomain": "awesome-store",
  "merchantEmail": "merchant@example.com",
  "merchantPhone": "+1-555-0100",
  "merchantBusinessName": "Awesome Store LLC",
  "ownerId": "user-guid"
}
```

**Response**: `201 Created` with StoreDto

**Validations**:
- Store name: 2-100 characters
- Subdomain: 3-63 characters, URL-safe format, unique
- Email: Valid email format
- Subdomain uniqueness check

---

#### UpdateStoreCommand
Updates existing store information and settings.

**Endpoint**: `PUT /api/stores/{id}`

**Request Body**:
```json
{
  "name": "Updated Store Name",
  "merchantEmail": "updated@example.com",
  "merchantPhone": "+1-555-0200",
  "currency": "USD",
  "timezone": "America/Los_Angeles",
  "taxRate": 8.5,
  "taxEnabled": true,
  "primaryColor": "#FF5733",
  "accentColor": "#C70039"
}
```

**Response**: `200 OK` with StoreDto

### Queries

#### GetStoreQuery
Retrieves a single store by ID or subdomain.

**Endpoints**:
- `GET /api/stores/{id}` - Get by ID
- `GET /api/stores/by-subdomain/{subdomain}` - Get by subdomain

**Response**: `200 OK` with StoreDto

---

#### GetStoresByMerchantQuery
Retrieves all stores owned by a specific merchant.

**Endpoint**: `GET /api/stores/by-owner/{ownerId}`

**Response**: `200 OK` with List&lt;StoreDto&gt;

## API Endpoints Summary

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/stores` | Create a new store |
| GET | `/api/stores/{id}` | Get store by ID |
| GET | `/api/stores/by-subdomain/{subdomain}` | Get store by subdomain |
| GET | `/api/stores/by-owner/{ownerId}` | Get all stores for a merchant |
| PUT | `/api/stores/{id}` | Update store information |
| GET | `/health` | Health check endpoint |

## Database Schema

### Stores Table

```sql
CREATE TABLE Stores (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Subdomain NVARCHAR(63) NOT NULL UNIQUE,
    OwnerId NVARCHAR(450) NOT NULL,
    Status NVARCHAR(20) NOT NULL,
    SubscriptionTier NVARCHAR(20) NOT NULL,

    -- Merchant Info (owned entity)
    MerchantEmail NVARCHAR(256) NOT NULL,
    MerchantPhone NVARCHAR(20),
    MerchantBusinessName NVARCHAR(200),
    MerchantAddress NVARCHAR(500),
    MerchantCity NVARCHAR(100),
    MerchantState NVARCHAR(100),
    MerchantPostalCode NVARCHAR(20),
    MerchantCountry NVARCHAR(100),

    -- Store Settings (owned entity)
    Currency NVARCHAR(3) NOT NULL,
    Timezone NVARCHAR(100) NOT NULL,
    Language NVARCHAR(10) NOT NULL,
    TaxRate DECIMAL(5,2) NOT NULL,
    TaxEnabled BIT NOT NULL,
    PrimaryColor NVARCHAR(7),
    AccentColor NVARCHAR(7),
    LogoUrl NVARCHAR(2048),

    -- Trial & Subscription
    TrialEndsAt DATETIME2,
    SubscriptionRenewedAt DATETIME2,

    -- Audit Fields
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2,
    CreatedBy NVARCHAR(450),
    UpdatedBy NVARCHAR(450),

    INDEX IX_Stores_Subdomain (Subdomain),
    INDEX IX_Stores_OwnerId (OwnerId)
);
```

## Validation Rules

### Store Creation
1. **Store Name**: Required, 2-100 characters
2. **Subdomain**:
   - Required, 3-63 characters
   - Lowercase letters, numbers, hyphens only
   - Must start/end with alphanumeric
   - Unique across all stores
   - Not in reserved list
3. **Merchant Email**: Required, valid email format
4. **Owner ID**: Required

### Store Update
1. **Store Name**: Required, 2-100 characters
2. **Merchant Email**: Required, valid email format
3. **Tax Rate**: 0-100 (if provided)
4. **Colors**: Valid hex format (if provided)

## Error Handling

The API uses a global exception handling middleware that returns standardized error responses:

```json
{
  "message": "Error description",
  "errors": ["Validation error 1", "Validation error 2"]
}
```

**HTTP Status Codes**:
- `200 OK`: Successful request
- `201 Created`: Resource created successfully
- `400 Bad Request`: Validation errors or invalid data
- `404 Not Found`: Resource not found
- `409 Conflict`: Subdomain already exists
- `500 Internal Server Error`: Unexpected server error

## Logging

The service uses **Serilog** for structured logging:
- Console sink for development
- File sink (`logs/tenant-management-*.log`) with daily rolling
- Request logging middleware
- Detailed error logging in handlers

## Getting Started

### Prerequisites
- .NET 9.0 SDK
- SQL Server (LocalDB or full instance)

### Setup

1. **Update Connection String**
   Edit `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER;Database=VendoTenantManagement;Trusted_Connection=true"
     }
   }
   ```

2. **Create Database Migration**
   ```bash
   cd src/Api
   dotnet ef migrations add InitialCreate --project ../Infrastructure --startup-project .
   ```

3. **Apply Migration**
   ```bash
   dotnet ef database update --project ../Infrastructure --startup-project .
   ```

4. **Run the API**
   ```bash
   dotnet run --project src/Api
   ```

5. **Access Swagger UI**
   Navigate to: `https://localhost:5001` or `http://localhost:5000`

## Testing Examples

### Create Store
```bash
curl -X POST https://localhost:5001/api/stores \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test Store",
    "subdomain": "test-store",
    "merchantEmail": "test@example.com",
    "ownerId": "test-owner-123"
  }'
```

### Get Store by Subdomain
```bash
curl https://localhost:5001/api/stores/by-subdomain/test-store
```

### Update Store
```bash
curl -X PUT https://localhost:5001/api/stores/{store-id} \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Updated Store Name",
    "merchantEmail": "test@example.com",
    "taxRate": 8.5,
    "taxEnabled": true
  }'
```

## Integration Points for Frontend

### Merchant Onboarding Flow
1. **Registration Page**: Call `POST /api/stores` with merchant details
2. **Subdomain Availability**: Can validate subdomain before submission
3. **Store Dashboard**: Call `GET /api/stores/by-owner/{ownerId}` to list stores
4. **Store Settings**: Call `PUT /api/stores/{id}` to update store configuration

### Store Context Resolution
Frontend can determine which store context by:
1. Parsing subdomain from URL
2. Calling `GET /api/stores/by-subdomain/{subdomain}`
3. Using returned store data for tenant-scoped operations

## Best Practices

1. **Always validate subdomain** on the client side before submission
2. **Use OwnerId from authentication context** (JWT claims)
3. **Implement retry logic** for transient database failures
4. **Cache store settings** on the client side with appropriate invalidation
5. **Log all store creation/updates** for audit purposes

## Future Enhancements

1. Store deletion/archival endpoints
2. Subscription upgrade/downgrade workflows
3. Custom domain management (CNAME configuration)
4. Store analytics and usage metrics
5. Bulk operations for platform administrators
6. Multi-owner support for collaborative stores

## License

Copyright (c) 2025 Vendo Platform
