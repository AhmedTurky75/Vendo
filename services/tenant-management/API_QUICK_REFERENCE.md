# Tenant Management API - Quick Reference Guide

## Base URL
```
Development: https://localhost:5001/api
Production: https://api.vendo.app/tenant-management/api
```

## Authentication
All endpoints (except health check) require JWT bearer token:
```
Authorization: Bearer {access_token}
```

---

## API Endpoints

### 1. Create Store
**Endpoint**: `POST /api/stores`

**Description**: Creates a new store for a merchant.

**Request Body**:
```json
{
  "name": "string (2-100 chars, required)",
  "subdomain": "string (3-63 chars, alphanumeric + hyphens, required)",
  "merchantEmail": "string (valid email, required)",
  "merchantPhone": "string (max 20 chars, optional)",
  "merchantBusinessName": "string (max 200 chars, optional)",
  "merchantAddress": "string (optional)",
  "merchantCity": "string (optional)",
  "merchantState": "string (optional)",
  "merchantPostalCode": "string (optional)",
  "merchantCountry": "string (optional)",
  "ownerId": "string (user ID from JWT, required)"
}
```

**Success Response**: `201 Created`
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "My Awesome Store",
  "subdomain": "awesome-store",
  "status": "Active",
  "subscriptionTier": "Free",
  "ownerId": "user-123",
  "merchantEmail": "merchant@example.com",
  "merchantPhone": "+1-555-0100",
  "merchantBusinessName": "Awesome Store LLC",
  "currency": "USD",
  "timezone": "America/New_York",
  "isInTrial": true,
  "trialEndsAt": "2025-11-05T10:00:00Z",
  "createdAt": "2025-10-22T10:00:00Z",
  "updatedAt": null
}
```

**Error Responses**:
- `400 Bad Request`: Validation errors
- `409 Conflict`: Subdomain already exists

---

### 2. Get Store by ID
**Endpoint**: `GET /api/stores/{id}`

**Description**: Retrieves store details by ID.

**Path Parameters**:
- `id` (GUID): Store unique identifier

**Success Response**: `200 OK`
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "My Awesome Store",
  "subdomain": "awesome-store",
  "status": "Active",
  "subscriptionTier": "Free",
  "ownerId": "user-123",
  "merchantEmail": "merchant@example.com",
  "merchantPhone": "+1-555-0100",
  "merchantBusinessName": "Awesome Store LLC",
  "currency": "USD",
  "timezone": "America/New_York",
  "isInTrial": true,
  "trialEndsAt": "2025-11-05T10:00:00Z",
  "createdAt": "2025-10-22T10:00:00Z",
  "updatedAt": null
}
```

**Error Responses**:
- `404 Not Found`: Store not found

---

### 3. Get Store by Subdomain
**Endpoint**: `GET /api/stores/by-subdomain/{subdomain}`

**Description**: Retrieves store details by subdomain.

**Path Parameters**:
- `subdomain` (string): Store subdomain (e.g., "awesome-store")

**Success Response**: `200 OK`
(Same as Get Store by ID)

**Error Responses**:
- `404 Not Found`: Store not found

---

### 4. Get Stores by Owner
**Endpoint**: `GET /api/stores/by-owner/{ownerId}`

**Description**: Retrieves all stores owned by a specific merchant.

**Path Parameters**:
- `ownerId` (string): Owner user ID

**Success Response**: `200 OK`
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Store 1",
    "subdomain": "store-1",
    "status": "Active",
    "subscriptionTier": "Pro",
    // ... other fields
  },
  {
    "id": "4gb96g75-6828-5673-c4gd-3d074g77bgb7",
    "name": "Store 2",
    "subdomain": "store-2",
    "status": "Active",
    "subscriptionTier": "Free",
    // ... other fields
  }
]
```

**Error Responses**:
- `400 Bad Request`: Invalid owner ID

---

### 5. Update Store
**Endpoint**: `PUT /api/stores/{id}`

**Description**: Updates store information and settings.

**Path Parameters**:
- `id` (GUID): Store unique identifier

**Request Body** (all fields optional except name and merchantEmail):
```json
{
  "name": "string (2-100 chars, required)",
  "merchantEmail": "string (valid email, required)",
  "merchantPhone": "string (max 20 chars, optional)",
  "merchantBusinessName": "string (max 200 chars, optional)",
  "merchantAddress": "string (optional)",
  "merchantCity": "string (optional)",
  "merchantState": "string (optional)",
  "merchantPostalCode": "string (optional)",
  "merchantCountry": "string (optional)",
  "currency": "string (ISO 4217, optional)",
  "timezone": "string (IANA timezone, optional)",
  "language": "string (ISO 639-1, optional)",
  "taxRate": "decimal (0-100, optional)",
  "taxEnabled": "boolean (optional)",
  "primaryColor": "string (hex format, optional)",
  "accentColor": "string (hex format, optional)",
  "logoUrl": "string (URL, optional)"
}
```

**Success Response**: `200 OK`
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Updated Store Name",
  "subdomain": "awesome-store",
  // ... updated fields
}
```

**Error Responses**:
- `400 Bad Request`: Validation errors
- `404 Not Found`: Store not found

---

### 6. Health Check
**Endpoint**: `GET /health`

**Description**: Health check endpoint for monitoring.

**Success Response**: `200 OK`
```json
{
  "status": "Healthy"
}
```

---

## Common Response Formats

### Success Response
All successful responses return the requested data with appropriate HTTP status code (200, 201).

### Error Response
All errors return a standardized error object:
```json
{
  "message": "Error description",
  "errors": [
    "Detailed error 1",
    "Detailed error 2"
  ]
}
```

---

## HTTP Status Codes

| Code | Description | Usage |
|------|-------------|-------|
| 200 | OK | Successful GET, PUT requests |
| 201 | Created | Successful POST (store created) |
| 400 | Bad Request | Validation errors, invalid data |
| 401 | Unauthorized | Missing or invalid authentication token |
| 404 | Not Found | Store not found |
| 409 | Conflict | Subdomain already exists |
| 500 | Internal Server Error | Unexpected server error |

---

## Validation Rules

### Subdomain
- **Length**: 3-63 characters
- **Format**: Lowercase letters, numbers, hyphens only
- **Pattern**: Must start and end with alphanumeric character
- **Uniqueness**: Must be unique across all stores
- **Reserved**: Cannot use: admin, api, www, app, mail, ftp, localhost, vendo, test, dev, stage, staging, prod, production, dashboard, billing, support, help, docs, blog

### Store Name
- **Length**: 2-100 characters
- **Required**: Yes

### Email
- **Format**: Valid email address format
- **Required**: Yes

### Tax Rate
- **Range**: 0-100
- **Decimals**: Up to 2 decimal places

### Colors
- **Format**: Hex color code (#RGB or #RRGGBB)
- **Examples**: #FFF, #FF5733, #C70039

---

## Example Requests

### cURL Examples

#### Create Store
```bash
curl -X POST "https://localhost:5001/api/stores" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -d '{
    "name": "Test Store",
    "subdomain": "test-store",
    "merchantEmail": "test@example.com",
    "ownerId": "user-123"
  }'
```

#### Get Store by Subdomain
```bash
curl -X GET "https://localhost:5001/api/stores/by-subdomain/test-store" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

#### Update Store
```bash
curl -X PUT "https://localhost:5001/api/stores/3fa85f64-5717-4562-b3fc-2c963f66afa6" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -d '{
    "name": "Updated Store Name",
    "merchantEmail": "test@example.com",
    "taxRate": 8.5,
    "taxEnabled": true,
    "primaryColor": "#FF5733"
  }'
```

---

### JavaScript/TypeScript Examples

#### Create Store
```typescript
const createStore = async (storeData: CreateStoreRequest) => {
  const response = await fetch('/api/stores', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${accessToken}`
    },
    body: JSON.stringify(storeData)
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.message);
  }

  return await response.json();
};
```

#### Get Store by Subdomain
```typescript
const getStoreBySubdomain = async (subdomain: string) => {
  const response = await fetch(`/api/stores/by-subdomain/${subdomain}`, {
    headers: {
      'Authorization': `Bearer ${accessToken}`
    }
  });

  if (!response.ok) {
    throw new Error('Store not found');
  }

  return await response.json();
};
```

#### Get User's Stores
```typescript
const getUserStores = async (userId: string) => {
  const response = await fetch(`/api/stores/by-owner/${userId}`, {
    headers: {
      'Authorization': `Bearer ${accessToken}`
    }
  });

  return await response.json();
};
```

---

## Integration Patterns

### Pattern 1: Store Context Resolution
When user visits a store-specific URL (e.g., awesome-store.vendo.app):

```typescript
// 1. Extract subdomain from URL
const subdomain = window.location.hostname.split('.')[0];

// 2. Fetch store context
const store = await getStoreBySubdomain(subdomain);

// 3. Store in application state
dispatch(setCurrentStore(store));

// 4. Use store.id for all subsequent tenant-scoped API calls
```

---

### Pattern 2: Merchant Dashboard
When merchant views their stores:

```typescript
// 1. Get user ID from JWT
const userId = getCurrentUserId();

// 2. Fetch all user's stores
const stores = await getUserStores(userId);

// 3. Display store list
renderStoreList(stores);

// 4. Allow selection/switching between stores
```

---

### Pattern 3: Store Creation Flow
During merchant onboarding:

```typescript
// 1. Collect data from registration form
const formData = {
  name: 'My Store',
  subdomain: 'my-store',
  merchantEmail: 'merchant@example.com',
  ownerId: currentUserId
};

// 2. Validate subdomain client-side (optional)
if (!isValidSubdomain(formData.subdomain)) {
  showError('Invalid subdomain format');
  return;
}

// 3. Create store
try {
  const store = await createStore(formData);
  showSuccess('Store created successfully!');
  navigateTo(`/dashboard/${store.id}`);
} catch (error) {
  if (error.status === 409) {
    showError('Subdomain already taken');
  } else {
    showError('Failed to create store');
  }
}
```

---

## Rate Limiting

**Note**: Rate limiting should be implemented at the API Gateway level.

**Recommended Limits**:
- Anonymous: 10 requests/minute
- Authenticated: 100 requests/minute
- Admin: 1000 requests/minute

---

## Monitoring & Debugging

### Health Check
Monitor service health:
```bash
curl https://api.vendo.app/tenant-management/health
```

### Logs
Service logs are available at:
- Console output (Development)
- File logs: `logs/tenant-management-*.log`
- Application Insights (Production)

### Swagger UI
Interactive API documentation:
```
Development: https://localhost:5001
Production: https://api.vendo.app/tenant-management
```

---

## Support

For API support or issues:
- Email: support@vendo.app
- Documentation: https://docs.vendo.app
- Status Page: https://status.vendo.app
