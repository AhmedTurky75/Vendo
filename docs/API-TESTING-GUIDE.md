# Vendo API Testing Guide

Complete guide for testing all Vendo backend services using the provided Postman collection.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Setup](#setup)
3. [Service Overview](#service-overview)
4. [Using the Postman Collection](#using-the-postman-collection)
5. [Test Scenarios](#test-scenarios)
6. [Troubleshooting](#troubleshooting)

## Prerequisites

### Required Software

- **.NET 9.0 SDK** - For running backend services
- **SQL Server** - Database server (SQL Server 2019+ or Azure SQL)
- **Postman** - API testing tool
- **Git** - Version control

### Optional Software

- **SQL Server Management Studio (SSMS)** - For database management
- **Visual Studio 2022** or **VS Code** - For code inspection

## Setup

### 1. Database Configuration

Each service requires a SQL Server database. Update the connection strings in `appsettings.json` for each service:

**services/tenant-management/src/Api/appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=VendoTenantManagement;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**services/catalog/src/Api/appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=VendoCatalog;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**services/order/src/Api/appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=VendoOrder;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**services/payment/src/Api/appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=VendoPayment;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 2. Trust Development Certificates

```bash
dotnet dev-certs https --trust
```

### 3. Start All Services

Open 4 separate terminal windows and run each service:

**Terminal 1 - Tenant Management Service (Port 5005):**
```bash
cd services/tenant-management/src/Api
dotnet run
```

**Terminal 2 - Catalog Service (Port 5002):**
```bash
cd services/catalog/src/Api
dotnet run
```

**Terminal 3 - Order Service (Port 5003):**
```bash
cd services/order/src/Api
dotnet run
```

**Terminal 4 - Payment Service (Port 5004):**
```bash
cd services/payment/src/Api
dotnet run
```

### 4. Verify Services are Running

Each service will automatically:
- Apply database migrations
- Seed initial data
- Start the API server
- Display the Swagger UI URL

Check the logs to ensure seeding completed successfully:
```
[Information] Applying database migrations...
[Information] Database migrations applied successfully.
[Information] Starting database seeding...
[Information] Seeded X records successfully.
[Information] Database seeding completed successfully.
```

### 5. Access Swagger UI

Each service has a Swagger UI available at:

- **Tenant Management**: https://localhost:5005
- **Catalog**: https://localhost:5002
- **Order**: https://localhost:5003
- **Payment**: https://localhost:5004

## Service Overview

### Tenant Management Service (Port 5005)

**Purpose**: Manages stores/tenants in the multi-tenant system

**Key Entities**:
- Store (with Subdomain, MerchantInfo, StoreSettings)

**API Endpoints**:
- `GET /api/stores` - Get all stores
- `GET /api/stores/{id}` - Get store by ID
- `POST /api/stores` - Create new store
- `PUT /api/stores/{id}` - Update store
- `DELETE /api/stores/{id}` - Delete store

**Seeded Data**: 3 stores
- TechGadgets (ID: `11111111-1111-1111-1111-111111111111`)
- Fashion Hub (ID: `22222222-2222-2222-2222-222222222222`)
- Home Essentials (ID: `33333333-3333-3333-3333-333333333333`)

### Catalog Service (Port 5002)

**Purpose**: Manages products and categories

**Key Entities**:
- Product
- Category (hierarchical)

**API Endpoints**:

**Categories:**
- `GET /api/categories?tenantId={guid}` - Get all categories
- `GET /api/categories/{id}` - Get category by ID
- `POST /api/categories` - Create category
- `PUT /api/categories/{id}` - Update category
- `DELETE /api/categories/{id}` - Delete category

**Products:**
- `GET /api/products?tenantId={guid}&page=1&pageSize=10` - Get all products (paginated)
- `GET /api/products/{id}` - Get product by ID
- `GET /api/products/search?tenantId={guid}&searchTerm={term}` - Search products
- `GET /api/products/category/{categoryId}?page=1&pageSize=10` - Get products by category
- `POST /api/products` - Create product
- `PUT /api/products/{id}` - Update product
- `DELETE /api/products/{id}` - Delete product

**Seeded Data**:
- 8 categories (Electronics, Smartphones, Laptops, Audio, Clothing, Accessories, Furniture, Kitchen)
- 9 products (iPhone, Samsung Galaxy, MacBook, Dell XPS, Sony Headphones, T-Shirt, Bag, Sofa, Cookware)

### Order Service (Port 5003)

**Purpose**: Manages customer orders

**Key Entities**:
- Order
- OrderItem

**API Endpoints**:
- `GET /api/orders?tenantId={guid}&page=1&pageSize=10` - Get all orders
- `GET /api/orders/{id}` - Get order by ID
- `GET /api/orders/customer/{customerId}` - Get orders by customer
- `GET /api/orders/status/{status}` - Get orders by status
- `POST /api/orders` - Create order
- `PUT /api/orders/{id}/status` - Update order status
- `POST /api/orders/{id}/cancel` - Cancel order

**Order Statuses**: Pending, Processing, Shipped, Delivered, Cancelled

**Seeded Data**: 6 orders with various statuses

### Payment Service (Port 5004)

**Purpose**: Manages payment transactions

**Key Entities**:
- Payment
- Transaction

**API Endpoints**:
- `GET /api/payments?tenantId={guid}&page=1&pageSize=10` - Get all payments
- `GET /api/payments/{id}` - Get payment by ID
- `GET /api/payments/order/{orderId}` - Get payment by order ID
- `POST /api/payments` - Create payment
- `POST /api/payments/{id}/process` - Process payment
- `POST /api/payments/{id}/refund` - Refund payment

**Payment Statuses**: Pending, Processing, Completed, Failed, Refunded, PartiallyRefunded, Cancelled

**Payment Methods**: CreditCard, DebitCard, PayPal, BankTransfer, Cash

**Seeded Data**: 7 payments with various statuses (Completed, Pending, Refunded, Failed)

## Using the Postman Collection

### 1. Import the Collection

1. Open Postman
2. Click **Import** button
3. Select **File** tab
4. Choose `postman/Vendo-API-Collection.postman_collection.json`
5. Click **Import**

### 2. Collection Variables

The collection includes pre-configured variables:

| Variable | Value | Description |
|----------|-------|-------------|
| `base_url_tenant` | `https://localhost:5005` | Tenant Management Service |
| `base_url_catalog` | `https://localhost:5002` | Catalog Service |
| `base_url_order` | `https://localhost:5003` | Order Service |
| `base_url_payment` | `https://localhost:5004` | Payment Service |
| `tenant_id_techgadgets` | `11111111-1111-1111-1111-111111111111` | TechGadgets Store ID |
| `tenant_id_fashionhub` | `22222222-2222-2222-2222-222222222222` | Fashion Hub Store ID |
| `tenant_id_homeessentials` | `33333333-3333-3333-3333-333333333333` | Home Essentials Store ID |

### 3. SSL Certificate Warnings

Since services use self-signed certificates, you may need to:

1. Go to **Postman Settings** > **General**
2. Turn **OFF** "SSL certificate verification"

## Test Scenarios

### Scenario 1: Complete Order Flow

This scenario tests the complete customer journey from browsing products to payment.

#### Step 1: Browse Products (Catalog Service)

**Request**: Get All Products for TechGadgets
```
GET https://localhost:5002/api/products?tenantId=11111111-1111-1111-1111-111111111111&page=1&pageSize=10
```

**Expected Response**: HTTP 200 with list of products
```json
{
  "items": [
    {
      "id": "p1111111-1111-1111-1111-111111111111",
      "name": "iPhone 15 Pro Max",
      "price": 1199.99,
      "stockQuantity": 50
      // ... more fields
    }
  ],
  "totalCount": 5,
  "page": 1,
  "pageSize": 10
}
```

#### Step 2: Search for Specific Product

**Request**: Search Products
```
GET https://localhost:5002/api/products/search?tenantId=11111111-1111-1111-1111-111111111111&searchTerm=iphone
```

**Expected Response**: HTTP 200 with filtered products matching "iphone"

#### Step 3: Get Product Details

**Request**: Get Product by ID
```
GET https://localhost:5002/api/products/p1111111-1111-1111-1111-111111111111
```

**Expected Response**: HTTP 200 with complete product details

#### Step 4: Create Order

**Request**: Create Order
```
POST https://localhost:5003/api/orders
Content-Type: application/json

{
  "tenantId": "11111111-1111-1111-1111-111111111111",
  "customerId": "u1111111-1111-1111-1111-111111111111",
  "customerName": "John Doe",
  "customerEmail": "john.doe@example.com",
  "customerPhone": "+1-555-0101",
  "shippingAddress": "{\"fullName\":\"John Doe\",\"addressLine1\":\"123 Tech Street\",\"city\":\"San Francisco\",\"state\":\"CA\",\"postalCode\":\"94102\",\"country\":\"USA\"}",
  "billingAddress": "{\"fullName\":\"John Doe\",\"addressLine1\":\"123 Tech Street\",\"city\":\"San Francisco\",\"state\":\"CA\",\"postalCode\":\"94102\",\"country\":\"USA\"}",
  "orderItems": [
    {
      "productId": "p1111111-1111-1111-1111-111111111111",
      "productName": "iPhone 15 Pro Max",
      "productSKU": "TECH-IPHONE-15-PM-256",
      "quantity": 1,
      "unitPrice": 1199.99
    }
  ],
  "subTotal": 1199.99,
  "taxAmount": 102.00,
  "shippingCost": 15.00,
  "discountAmount": 0.00,
  "totalAmount": 1316.99
}
```

**Expected Response**: HTTP 201 with created order details including Order ID

#### Step 5: Create Payment

**Request**: Create Payment
```
POST https://localhost:5004/api/payments
Content-Type: application/json

{
  "tenantId": "11111111-1111-1111-1111-111111111111",
  "orderId": "[USE ORDER ID FROM STEP 4]",
  "customerId": "u1111111-1111-1111-1111-111111111111",
  "amount": 1316.99,
  "currency": "USD",
  "paymentMethod": "CreditCard"
}
```

**Expected Response**: HTTP 201 with created payment (Status: Pending)

#### Step 6: Process Payment

**Request**: Process Payment
```
POST https://localhost:5004/api/payments/[PAYMENT_ID]/process
Content-Type: application/json

{
  "transactionId": "stripe_pi_test_123456789",
  "gatewayResponse": "{\"status\":\"succeeded\"}"
}
```

**Expected Response**: HTTP 200 with updated payment (Status: Completed)

#### Step 7: Update Order Status

**Request**: Update Order Status to Processing
```
PUT https://localhost:5003/api/orders/[ORDER_ID]/status
Content-Type: application/json

{
  "status": "Processing"
}
```

**Expected Response**: HTTP 200 with updated order

#### Step 8: Ship Order

**Request**: Update Order Status to Shipped
```
PUT https://localhost:5003/api/orders/[ORDER_ID]/status
Content-Type: application/json

{
  "status": "Shipped"
}
```

**Expected Response**: HTTP 200 with updated order

### Scenario 2: Multi-Tenant Isolation

This scenario verifies that data is properly isolated between tenants.

#### Step 1: Get Products for TechGadgets

```
GET https://localhost:5002/api/products?tenantId=11111111-1111-1111-1111-111111111111&page=1&pageSize=10
```

**Expected**: Returns only electronics products

#### Step 2: Get Products for Fashion Hub

```
GET https://localhost:5002/api/products?tenantId=22222222-2222-2222-2222-222222222222&page=1&pageSize=10
```

**Expected**: Returns only fashion products (different from Step 1)

#### Step 3: Get Products for Home Essentials

```
GET https://localhost:5002/api/products?tenantId=33333333-3333-3333-3333-333333333333&page=1&pageSize=10
```

**Expected**: Returns only home goods products (different from Steps 1 & 2)

**Verification**: Ensure products are not shared across tenants

### Scenario 3: Order Cancellation and Refund

This scenario tests the cancellation and refund flow.

#### Step 1: Get an Existing Order

```
GET https://localhost:5003/api/orders/o1111111-1111-1111-1111-111111111111
```

**Expected**: HTTP 200 with order details

#### Step 2: Get Payment for the Order

```
GET https://localhost:5004/api/payments/order/o1111111-1111-1111-1111-111111111111
```

**Expected**: HTTP 200 with payment details (Status: Completed)

#### Step 3: Cancel the Order

```
POST https://localhost:5003/api/orders/o1111111-1111-1111-1111-111111111111/cancel
```

**Expected**: HTTP 200 with updated order (Status: Cancelled)

#### Step 4: Refund the Payment

```
POST https://localhost:5004/api/payments/[PAYMENT_ID]/refund
Content-Type: application/json

{
  "refundAmount": 1316.99,
  "reason": "Order cancelled by customer"
}
```

**Expected**: HTTP 200 with updated payment (Status: Refunded)

### Scenario 4: Product Catalog Management

This scenario tests CRUD operations on products and categories.

#### Step 1: Create New Category

```
POST https://localhost:5002/api/categories
Content-Type: application/json

{
  "tenantId": "11111111-1111-1111-1111-111111111111",
  "name": "Tablets",
  "description": "iPad and Android tablets",
  "slug": "tablets",
  "parentCategoryId": "c1111111-1111-1111-1111-111111111111",
  "displayOrder": 5,
  "isActive": true
}
```

**Expected**: HTTP 201 with created category

#### Step 2: Create Product in New Category

```
POST https://localhost:5002/api/products
Content-Type: application/json

{
  "tenantId": "11111111-1111-1111-1111-111111111111",
  "categoryId": "[USE CATEGORY ID FROM STEP 1]",
  "name": "iPad Pro 12.9-inch",
  "description": "Most powerful iPad with M2 chip",
  "shortDescription": "Professional tablet with M2",
  "sku": "TECH-IPAD-PRO-129-256",
  "price": 1099.99,
  "stockQuantity": 30,
  "status": "Active",
  "isFeatured": true
}
```

**Expected**: HTTP 201 with created product

#### Step 3: Update Product

```
PUT https://localhost:5002/api/products/[PRODUCT_ID]
Content-Type: application/json

{
  "name": "iPad Pro 12.9-inch (Updated)",
  "price": 999.99,
  "stockQuantity": 50
}
```

**Expected**: HTTP 200 with updated product

#### Step 4: Delete Product

```
DELETE https://localhost:5002/api/products/[PRODUCT_ID]
```

**Expected**: HTTP 204 No Content

#### Step 5: Verify Deletion

```
GET https://localhost:5002/api/products/[PRODUCT_ID]
```

**Expected**: HTTP 404 Not Found

### Scenario 5: Store/Tenant Management

This scenario tests store CRUD operations.

#### Step 1: Create New Store

```
POST https://localhost:5005/api/stores
Content-Type: application/json

{
  "name": "Pet Supplies Store",
  "subdomain": "petsupplies",
  "ownerId": "merchant-pet-owner-guid",
  "email": "owner@petsupplies.com",
  "phone": "+1-555-7777",
  "businessName": "Pet Supplies LLC",
  "address": "789 Pet Street",
  "city": "Seattle",
  "state": "WA",
  "postalCode": "98101",
  "country": "USA"
}
```

**Expected**: HTTP 201 with created store

#### Step 2: Get Store Details

```
GET https://localhost:5005/api/stores/[STORE_ID]
```

**Expected**: HTTP 200 with store details

#### Step 3: Update Store

```
PUT https://localhost:5005/api/stores/[STORE_ID]
Content-Type: application/json

{
  "name": "Premium Pet Supplies",
  "email": "updated@petsupplies.com"
}
```

**Expected**: HTTP 200 with updated store

#### Step 4: Get All Stores

```
GET https://localhost:5005/api/stores
```

**Expected**: HTTP 200 with list including the new store

## Error Testing

### Test Case 1: Invalid Product Creation (Missing Required Field)

```
POST https://localhost:5002/api/products
Content-Type: application/json

{
  "tenantId": "11111111-1111-1111-1111-111111111111",
  "categoryId": "c2222222-2222-2222-2222-222222222222",
  "name": ""  // Empty name - should fail validation
}
```

**Expected**: HTTP 400 Bad Request with validation errors

### Test Case 2: Duplicate SKU

```
POST https://localhost:5002/api/products
Content-Type: application/json

{
  "tenantId": "11111111-1111-1111-1111-111111111111",
  "categoryId": "c2222222-2222-2222-2222-222222222222",
  "name": "Test Product",
  "sku": "TECH-IPHONE-15-PM-256",  // Already exists
  "price": 999.99
}
```

**Expected**: HTTP 400 Bad Request with "SKU already exists" error

### Test Case 3: Non-Existent Resource

```
GET https://localhost:5002/api/products/99999999-9999-9999-9999-999999999999
```

**Expected**: HTTP 404 Not Found

### Test Case 4: Invalid Order Status Transition

```
PUT https://localhost:5003/api/orders/[DELIVERED_ORDER_ID]/status
Content-Type: application/json

{
  "status": "Pending"  // Can't go back to Pending from Delivered
}
```

**Expected**: HTTP 400 Bad Request

## Troubleshooting

### Issue: Service won't start

**Symptoms**: Service crashes on startup or shows connection errors

**Solutions**:
1. Verify SQL Server is running
2. Check connection strings in appsettings.json
3. Ensure no other application is using the port
4. Trust development certificates: `dotnet dev-certs https --trust`

### Issue: Seeding data not appearing

**Symptoms**: GET requests return empty lists

**Solutions**:
1. Check service logs for seeding errors
2. Verify JSON seed files exist in `Data/SeedData` directory
3. Drop database and restart service to re-seed:
   ```sql
   DROP DATABASE VendoCatalog;
   ```
4. Check database directly using SSMS to confirm tables exist

### Issue: SSL Certificate errors in Postman

**Solutions**:
1. Disable SSL verification: Settings > General > SSL certificate verification OFF
2. Or trust the certificate in your system

### Issue: Migrations not applying

**Symptoms**: Error about missing tables or columns

**Solutions**:
1. Check that connection string is correct
2. Ensure SQL Server is accessible
3. Create initial migration if missing:
   ```bash
   dotnet ef migrations add InitialCreate
   ```
4. Check service logs for migration errors

### Issue: Cross-tenant data leakage

**Symptoms**: Seeing products from wrong tenant

**Solutions**:
1. Always include `tenantId` query parameter
2. Verify tenant ID matches the store you're testing
3. Check database queries are filtering by TenantId

## Performance Testing

### Load Testing with Postman

1. Create a **Collection Runner** with the following:
   - Select "Vendo API Collection"
   - Set Iterations: 100
   - Set Delay: 50ms
   - Run collection

2. Monitor response times:
   - GET requests should be < 200ms
   - POST requests should be < 500ms
   - Complex queries should be < 1000ms

### Database Query Performance

1. Enable SQL Server Profiler
2. Run test scenarios
3. Look for:
   - N+1 query problems
   - Missing indexes
   - Full table scans

## Best Practices

1. **Always include tenant ID** when querying multi-tenant endpoints
2. **Use pagination** for list endpoints to avoid large responses
3. **Check response status codes** - don't just rely on response body
4. **Validate data** before creating/updating resources
5. **Clean up test data** after running test scenarios
6. **Use environment variables** in Postman for sensitive data
7. **Test error cases** as thoroughly as happy paths

## Additional Resources

- **Swagger Documentation**: Available at each service's root URL when running
- **Entity Relationship Diagrams**: See `/docs/ERD.md`
- **Architecture Documentation**: See `/docs/ARCHITECTURE.md`
- **Seeded Data Reference**: See JSON files in `services/*/Data/SeedData/`

## Support

For issues or questions:
1. Check service logs for detailed error messages
2. Review this testing guide
3. Consult Swagger documentation
4. Check database directly using SSMS
5. Review source code for business logic

---

**Last Updated**: 2025-10-24
**Version**: 1.0
