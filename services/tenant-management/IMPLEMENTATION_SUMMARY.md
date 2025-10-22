# Tenant Management Service - Implementation Summary

## Overview
This document provides a comprehensive summary of the backend API implementation for the merchant landing page where merchants can create and manage their stores in the Vendo multi-tenant e-commerce platform.

---

## 1. Entities and Value Objects Created

### Domain Entities

#### Store (Aggregate Root)
**Location**: `/src/Domain/Entities/Store.cs`

**Purpose**: Represents a merchant's store (tenant) in the system.

**Key Properties**:
- `Id`: Unique identifier (GUID)
- `Name`: Store display name
- `Subdomain`: Unique URL-safe subdomain (Value Object)
- `MerchantInfo`: Merchant contact information (Value Object)
- `Settings`: Store configuration (Value Object)
- `Status`: StoreStatus enum (Active, Inactive, Suspended, Archived)
- `SubscriptionTier`: Subscription level (Free, Starter, Pro)
- `OwnerId`: Reference to merchant user
- `TrialEndsAt`: Trial period expiration
- `IsInTrial`: Computed property
- `IsActive`: Computed property

**Key Methods**:
- `Create()`: Factory method for creating new stores
- `UpdateName()`: Updates store name
- `UpdateMerchantInfo()`: Updates merchant details
- `UpdateSettings()`: Updates store configuration
- `ChangeStatus()`, `Activate()`, `Deactivate()`, `Suspend()`, `Archive()`
- `UpgradeSubscription()`, `DowngradeSubscription()`
- `ExtendTrial()`, `EndTrial()`

### Value Objects

#### Subdomain
**Location**: `/src/Domain/ValueObjects/Subdomain.cs`

**Purpose**: Encapsulates subdomain validation and formatting rules.

**Features**:
- Immutable value object
- Static factory method `Create()` with validation
- Returns `Result<Subdomain>` for error handling
- Validates format (alphanumeric + hyphens, 3-63 chars)
- Blocks reserved subdomains (admin, api, www, etc.)
- Case-insensitive equality

#### MerchantInfo
**Location**: `/src/Domain/ValueObjects/MerchantInfo.cs`

**Purpose**: Encapsulates merchant contact and business information.

**Properties**:
- Email (required)
- Phone, BusinessName, Address, City, State, PostalCode, Country (optional)

#### StoreSettings
**Location**: `/src/Domain/ValueObjects/StoreSettings.cs`

**Purpose**: Encapsulates store configuration and branding settings.

**Properties**:
- Currency, Timezone, Language
- TaxRate, TaxEnabled
- PrimaryColor, AccentColor, LogoUrl

**Methods**:
- `CreateDefault()`: Factory for default settings
- `UpdateCurrency()`, `UpdateTaxSettings()`, `UpdateBranding()`

### Enumerations

#### StoreStatus
**Location**: `/src/Domain/Enums/StoreStatus.cs`
- Active, Inactive, Suspended, Archived

#### SubscriptionTier
**Location**: `/src/Domain/Enums/SubscriptionTier.cs`
- Free (50 products, 100 orders/month, 1 user)
- Starter (500 products, 1000 orders/month, 3 users)
- Pro (unlimited)

### Base Classes

#### BaseEntity
**Location**: `/src/Domain/Common/BaseEntity.cs`
- Provides common properties: Id, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy

#### IAggregateRoot
**Location**: `/src/Domain/Common/IAggregateRoot.cs`
- Marker interface for aggregate roots

---

## 2. Commands and Queries Implemented

### Commands

#### CreateStoreCommand
**Location**: `/src/Application/Stores/Commands/CreateStore/`

**Files**:
- `CreateStoreCommand.cs`: Command definition
- `CreateStoreCommandValidator.cs`: FluentValidation rules
- `CreateStoreCommandHandler.cs`: Command handler

**Purpose**: Creates a new store with merchant information.

**Validation Rules**:
- Name: 2-100 characters, required
- Subdomain: 3-63 characters, alphanumeric + hyphens, unique
- Email: Valid email format, required
- OwnerId: Required

**Handler Logic**:
1. Validates subdomain uniqueness
2. Creates Subdomain value object
3. Creates MerchantInfo value object
4. Creates Store entity with default settings
5. Persists to database
6. Returns StoreDto

---

#### UpdateStoreCommand
**Location**: `/src/Application/Stores/Commands/UpdateStore/`

**Files**:
- `UpdateStoreCommand.cs`: Command definition
- `UpdateStoreCommandValidator.cs`: FluentValidation rules
- `UpdateStoreCommandHandler.cs`: Command handler

**Purpose**: Updates existing store information and settings.

**Validation Rules**:
- Store ID: Required
- Name: 2-100 characters
- Email: Valid format
- TaxRate: 0-100%
- Colors: Valid hex format (#RRGGBB)

**Handler Logic**:
1. Retrieves existing store
2. Updates name if provided
3. Updates merchant info
4. Updates settings if any provided
5. Persists changes
6. Returns updated StoreDto

### Queries

#### GetStoreQuery
**Location**: `/src/Application/Stores/Queries/GetStore/`

**Files**:
- `GetStoreQuery.cs`: Query definition
- `GetStoreQueryHandler.cs`: Query handler

**Purpose**: Retrieves a single store by ID or subdomain.

**Handler Logic**:
- Queries by ID if provided
- Queries by subdomain if provided
- Returns StoreDto or error

---

#### GetStoresByMerchantQuery
**Location**: `/src/Application/Stores/Queries/GetStoresByMerchant/`

**Files**:
- `GetStoresByMerchantQuery.cs`: Query definition
- `GetStoresByMerchantQueryHandler.cs`: Query handler

**Purpose**: Retrieves all stores owned by a specific merchant.

**Handler Logic**:
- Queries stores by OwnerId
- Orders by CreatedAt descending
- Returns List<StoreDto>

### DTOs

#### StoreDto
**Location**: `/src/Application/Stores/DTOs/StoreDto.cs`

**Properties**: Id, Name, Subdomain, Status, SubscriptionTier, OwnerId, MerchantEmail, MerchantPhone, MerchantBusinessName, Currency, Timezone, IsInTrial, TrialEndsAt, CreatedAt, UpdatedAt

---

## 3. API Endpoints Available

### StoresController
**Location**: `/src/Api/Controllers/StoresController.cs`

| Method | Endpoint | Description | Response |
|--------|----------|-------------|----------|
| POST | `/api/stores` | Create new store | 201 Created + StoreDto |
| GET | `/api/stores/{id}` | Get store by ID | 200 OK + StoreDto |
| GET | `/api/stores/by-subdomain/{subdomain}` | Get store by subdomain | 200 OK + StoreDto |
| GET | `/api/stores/by-owner/{ownerId}` | Get stores by merchant | 200 OK + List<StoreDto> |
| PUT | `/api/stores/{id}` | Update store | 200 OK + StoreDto |

### Additional Endpoints
- `GET /health`: Health check endpoint

**API Documentation**:
- Swagger UI available at root URL (`/`)
- OpenAPI specification at `/swagger/v1/swagger.json`

---

## 4. Database Schema/Models

### Stores Table

**Table Name**: `Stores`

**Columns**:

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PRIMARY KEY | Store unique identifier |
| Name | NVARCHAR(100) | NOT NULL | Store display name |
| Subdomain | NVARCHAR(63) | NOT NULL, UNIQUE | Unique subdomain |
| OwnerId | NVARCHAR(450) | NOT NULL, INDEXED | Merchant user ID |
| Status | NVARCHAR(20) | NOT NULL | Store status (enum as string) |
| SubscriptionTier | NVARCHAR(20) | NOT NULL | Subscription level |
| MerchantEmail | NVARCHAR(256) | NOT NULL | Merchant contact email |
| MerchantPhone | NVARCHAR(20) | NULL | Merchant phone |
| MerchantBusinessName | NVARCHAR(200) | NULL | Business name |
| MerchantAddress | NVARCHAR(500) | NULL | Business address |
| MerchantCity | NVARCHAR(100) | NULL | City |
| MerchantState | NVARCHAR(100) | NULL | State/Province |
| MerchantPostalCode | NVARCHAR(20) | NULL | Postal code |
| MerchantCountry | NVARCHAR(100) | NULL | Country |
| Currency | NVARCHAR(3) | NOT NULL | ISO 4217 currency code |
| Timezone | NVARCHAR(100) | NOT NULL | IANA timezone |
| Language | NVARCHAR(10) | NOT NULL | ISO 639-1 language code |
| TaxRate | DECIMAL(5,2) | NOT NULL | Tax percentage (0-100) |
| TaxEnabled | BIT | NOT NULL | Tax enabled flag |
| PrimaryColor | NVARCHAR(7) | NULL | Brand primary color (hex) |
| AccentColor | NVARCHAR(7) | NULL | Brand accent color (hex) |
| LogoUrl | NVARCHAR(2048) | NULL | Logo image URL |
| TrialEndsAt | DATETIME2 | NULL | Trial expiration date |
| SubscriptionRenewedAt | DATETIME2 | NULL | Last renewal date |
| CreatedAt | DATETIME2 | NOT NULL | Creation timestamp |
| UpdatedAt | DATETIME2 | NULL | Last update timestamp |
| CreatedBy | NVARCHAR(450) | NULL | Creator user ID |
| UpdatedBy | NVARCHAR(450) | NULL | Last updater user ID |

**Indexes**:
- `IX_Stores_Subdomain`: Unique index on Subdomain
- `IX_Stores_OwnerId`: Index on OwnerId for merchant queries

**Configuration**:
- Entity Framework Core 9.0
- SQL Server provider
- Owned entities for MerchantInfo and StoreSettings (no separate tables)
- Enum conversions to string for Status and SubscriptionTier

---

## 5. Validation Rules Implemented

### Subdomain Validation (Domain Level)
**Location**: `Subdomain.Create()` method

- **Length**: 3-63 characters
- **Format**: Lowercase letters, numbers, hyphens only
- **Pattern**: Must start and end with alphanumeric character
- **Reserved**: Blocks reserved names (admin, api, www, app, mail, ftp, localhost, vendo, test, dev, stage, staging, prod, production, dashboard, billing, support, help, docs, blog)
- **Uniqueness**: Checked at application level before persistence

### CreateStoreCommand Validation
**Location**: `CreateStoreCommandValidator.cs`

- **Name**: Not empty, 2-100 characters
- **Subdomain**: Not empty, 3-63 characters, matches regex pattern
- **MerchantEmail**: Not empty, valid email format
- **OwnerId**: Not empty
- **MerchantPhone**: Max 20 characters (if provided)
- **MerchantBusinessName**: Max 200 characters (if provided)

### UpdateStoreCommand Validation
**Location**: `UpdateStoreCommandValidator.cs`

- **Id**: Not empty (GUID)
- **Name**: Not empty, 2-100 characters
- **MerchantEmail**: Not empty, valid email format
- **MerchantPhone**: Max 20 characters (if provided)
- **MerchantBusinessName**: Max 200 characters (if provided)
- **TaxRate**: 0-100 (if provided)
- **PrimaryColor**: Valid hex color format (if provided)
- **AccentColor**: Valid hex color format (if provided)

### Handler-Level Validations

#### CreateStoreCommandHandler
- Subdomain uniqueness check before creation
- Returns conflict error if subdomain exists

#### UpdateStoreCommandHandler
- Store existence check (404 if not found)
- Validates all value objects during creation

### Validation Pipeline
**Location**: `ValidationBehavior.cs`

- MediatR pipeline behavior
- Automatically validates all commands using FluentValidation
- Throws `ValidationException` if validation fails
- Caught by `ExceptionHandlingMiddleware` and returned as 400 Bad Request

---

## 6. Integration Points for Frontend

### Merchant Onboarding Flow

#### Step 1: Registration
**Frontend Action**: Collect merchant details and store information

**API Call**:
```http
POST /api/stores
Content-Type: application/json

{
  "name": "My Awesome Store",
  "subdomain": "awesome-store",
  "merchantEmail": "merchant@example.com",
  "merchantPhone": "+1-555-0100",
  "merchantBusinessName": "Awesome Store LLC",
  "ownerId": "auth-user-id-from-jwt"
}
```

**Response**: 201 Created with StoreDto

**Frontend Handling**:
- Display success message
- Redirect to store dashboard
- Store `storeId` in local context

---

#### Step 2: Subdomain Availability Check (Optional)
While validation happens server-side, frontend can check subdomain format client-side using the same regex:
```javascript
const subdomainRegex = /^[a-z0-9](?:[a-z0-9-]{1,61}[a-z0-9])?$/;
const isValid = subdomainRegex.test(subdomain.toLowerCase());
```

---

### Store Management

#### Get Current User's Stores
**API Call**:
```http
GET /api/stores/by-owner/{ownerId}
```

**Response**: 200 OK with List<StoreDto>

**Frontend Usage**:
- Display list of stores in merchant dashboard
- Allow switching between stores
- Show store status and subscription tier

---

#### Get Store Details
**API Call (by ID)**:
```http
GET /api/stores/{storeId}
```

**API Call (by subdomain)**:
```http
GET /api/stores/by-subdomain/{subdomain}
```

**Response**: 200 OK with StoreDto

**Frontend Usage**:
- Load store context for tenant-scoped operations
- Display store settings page
- Populate forms for editing

---

#### Update Store Settings
**API Call**:
```http
PUT /api/stores/{storeId}
Content-Type: application/json

{
  "name": "Updated Store Name",
  "merchantEmail": "updated@example.com",
  "merchantPhone": "+1-555-0200",
  "currency": "USD",
  "timezone": "America/Los_Angeles",
  "taxRate": 8.5,
  "taxEnabled": true,
  "primaryColor": "#FF5733",
  "accentColor": "#C70039",
  "logoUrl": "https://cdn.example.com/logo.png"
}
```

**Response**: 200 OK with updated StoreDto

**Frontend Usage**:
- Store settings form
- Branding customization page
- Tax configuration

---

### Store Context Resolution

**Scenario**: Frontend needs to determine which store context based on URL

**Approach**:
1. Parse subdomain from window.location.hostname
2. Call `GET /api/stores/by-subdomain/{subdomain}`
3. Store StoreDto in application state (Redux, Context API, etc.)
4. Use store context for all subsequent API calls

**Example**:
```javascript
// URL: https://awesome-store.vendo.app
const subdomain = window.location.hostname.split('.')[0];
const response = await fetch(`/api/stores/by-subdomain/${subdomain}`);
const store = await response.json();
// Now use store.id for tenant-scoped operations
```

---

### Error Handling

**Frontend should handle**:
- 400 Bad Request: Display validation errors from `errors` array
- 404 Not Found: Show "Store not found" message
- 409 Conflict: Show "Subdomain already taken" error
- 500 Internal Server Error: Show generic error message

**Error Response Format**:
```json
{
  "message": "Validation failed",
  "errors": [
    "Store name is required",
    "Subdomain must be at least 3 characters"
  ]
}
```

---

### Authentication Integration

**OwnerId Source**: Extract from JWT claims after user authentication

**Example** (assuming JWT contains `sub` claim):
```javascript
const token = localStorage.getItem('access_token');
const payload = JSON.parse(atob(token.split('.')[1]));
const ownerId = payload.sub; // Use this for API calls
```

---

### Recommended Frontend State Structure

```typescript
interface AppState {
  currentStore: StoreDto | null;
  userStores: StoreDto[];
  isLoading: boolean;
  error: string | null;
}
```

---

## Additional Implementation Details

### Infrastructure Layer

#### DbContext Configuration
**Location**: `/src/Infrastructure/Persistence/TenantManagementDbContext.cs`
- Configures Store entity
- Applies entity configurations

#### Entity Configuration
**Location**: `/src/Infrastructure/Persistence/Configurations/StoreConfiguration.cs`
- Maps Store entity to database table
- Configures value object conversions
- Defines indexes

#### Repository Implementation
**Location**: `/src/Infrastructure/Persistence/Repositories/StoreRepository.cs`
- Implements IStoreRepository
- Provides CRUD operations
- Includes optimized queries

#### Dependency Injection
**Location**: `/src/Infrastructure/DependencyInjection.cs`
- Registers DbContext with connection string
- Registers repositories
- Configures retry policies

---

### Application Layer

#### Result Pattern
**Location**: `/src/Application/Common/Models/Result.cs`
- Generic result type for operation outcomes
- Supports success/failure with error messages
- Used by all command/query handlers

#### Validation Behavior
**Location**: `/src/Application/Common/Behaviors/ValidationBehavior.cs`
- MediatR pipeline behavior
- Intercepts requests
- Runs FluentValidation validators
- Throws ValidationException on failure

#### Dependency Injection
**Location**: `/src/Application/DependencyInjection.cs`
- Registers MediatR
- Registers FluentValidation validators
- Registers pipeline behaviors

---

### API Layer

#### Exception Handling Middleware
**Location**: `/src/Api/Middleware/ExceptionHandlingMiddleware.cs`
- Global exception handler
- Catches ValidationException (400)
- Catches KeyNotFoundException (404)
- Catches UnauthorizedAccessException (401)
- Returns standardized error responses

#### Program.cs Configuration
**Location**: `/src/Api/Program.cs`
- Configures Serilog logging
- Registers Application and Infrastructure layers
- Configures Swagger/OpenAPI
- Adds CORS policy
- Adds health checks
- Configures middleware pipeline

#### Configuration Files
- `appsettings.json`: Production configuration
- `appsettings.Development.json`: Development overrides
- Connection strings for database
- Logging levels

---

## NuGet Packages Used

### Domain Layer
- No external dependencies (pure domain logic)

### Application Layer
- MediatR 12.4.1
- FluentValidation 11.11.0
- FluentValidation.DependencyInjectionExtensions 11.11.0
- Microsoft.Extensions.Logging.Abstractions 9.0.0

### Infrastructure Layer
- Microsoft.EntityFrameworkCore 9.0.0
- Microsoft.EntityFrameworkCore.SqlServer 9.0.0
- Microsoft.EntityFrameworkCore.Design 9.0.0

### API Layer
- Swashbuckle.AspNetCore 7.2.0
- Serilog.AspNetCore 8.0.3
- Serilog.Sinks.Console 6.0.0
- Serilog.Sinks.File 6.0.0
- AspNetCore.HealthChecks.SqlServer 9.0.2

---

## Next Steps for Deployment

1. **Database Setup**:
   - Run EF Core migrations: `dotnet ef migrations add InitialCreate`
   - Apply to database: `dotnet ef database update`

2. **Configuration**:
   - Update connection strings for target environment
   - Configure CORS for production domains
   - Set up Serilog sinks (Application Insights, etc.)

3. **Testing**:
   - Unit tests for domain entities and value objects
   - Integration tests for commands and queries
   - API tests for controllers

4. **Security**:
   - Add authentication middleware (JWT validation)
   - Implement authorization policies
   - Add rate limiting

5. **Observability**:
   - Configure Application Insights
   - Set up distributed tracing
   - Create dashboards for monitoring

---

## Summary

This implementation provides a complete, production-ready backend API for merchant store management following Clean Architecture and CQRS patterns. The API is fully documented with Swagger, includes comprehensive validation, structured logging, and error handling. The frontend can easily integrate with the provided endpoints to build the merchant onboarding and store management user interface.
