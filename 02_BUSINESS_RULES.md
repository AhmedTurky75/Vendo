# 02_BUSINESS_RULES.md

## Purpose

This document defines the **business rules and domain invariants** that govern how the platform operates. These rules must be enforced across all services, ensuring predictable behavior and consistent domain logic regardless of technology or implementation changes.

These are the contracts between business intent and code — all AI agents and developers must respect these invariants in every implementation.

---

## Core Business Domains

1. **Tenant Management** — manages merchant stores, subscriptions, and configuration.
2. **Catalog Management** — handles products, categories, variants, and inventory.
3. **Order Management** — manages the lifecycle of customer orders.
4. **Payment Orchestration** — integrates with hosted payment providers.
5. **Identity & Access** — manages users, roles, and permissions.
6. **Platform Administration** — handles monitoring, support, and policy enforcement.

---

## 1. Tenant Management Rules

### Entity: Tenant

* Each merchant represents one **tenant**.
* Each tenant has a unique `TenantId`, store name, and subdomain.
* Store URL format: `{storeName}.{baseDomain}`.
* The system must isolate all merchant data logically (via `TenantId` field or separate DB in future).

### Rules

* Tenant creation must trigger:

  * Creation of a default admin user for the merchant.
  * Initialization of default store settings (currency, language, timezone, tax rate template).
* Tenant cannot be hard-deleted — use soft delete with `IsActive = false`.
* Tenant subscription level controls feature access (e.g., advanced analytics, multiple staff users).
* Store subdomain and merchant name must be unique.
* Store settings may include public branding, theme, and logo.

---

## 2. Catalog Management Rules

### Entity: Product

* A product belongs to exactly one tenant.
* A product can have multiple variants (e.g., size, color).
* A product can belong to multiple categories.
* A product must have at least one price and one stock record.

### Rules

* Product `SKU` must be unique per tenant.
* Product deletion is soft (set `IsActive = false`).
* Product must not be deletable if referenced in active orders.
* Inventory quantity must never go negative.
* Product publish status: `Draft`, `Published`, `Archived`.
* Only `Published` products are visible to storefront visitors.
* Changing price or variant triggers a version update event for analytics tracking.

---

## 3. Order Management Rules

### Entity: Order

* An order belongs to one tenant and one customer.
* Order statuses: `Pending`, `Confirmed`, `Paid`, `Shipped`, `Delivered`, `Cancelled`.

### Rules

* Orders can only transition between valid states:

  * `Pending → Confirmed`
  * `Confirmed → Paid`
  * `Paid → Shipped`
  * `Shipped → Delivered`
  * `Pending/Confirmed → Cancelled`
* Orders are immutable after being marked as `Paid`, except for shipment tracking updates.
* Order totals must be computed server-side only (never trust client data).
* Taxes and discounts are applied during checkout; totals must be recalculated if cart contents change.
* When payment is confirmed (via webhook), the order must automatically transition to `Paid`.
* Cancelled or refunded orders must restore inventory quantities.

---

## 4. Payment Orchestration Rules

### Entity: Payment

* Payment records reference an order and the external provider transaction ID.
* The platform never stores raw card data or CVV.
* Only tokenized or hosted-checkout payments are supported.

### Rules

* Each order may have one or more payment attempts, but only one can succeed.
* Payment flow:

  1. Create payment session (request hosted checkout URL).
  2. Redirect customer to hosted checkout.
  3. Provider calls back webhook on success/failure.
  4. Payment service validates signature and updates order/payment status.
* Webhooks must be idempotent and verified by provider signature.
* Payment record fields:

  * `PaymentId` (internal)
  * `OrderId`
  * `ProviderName`
  * `ProviderTransactionId`
  * `Amount`
  * `Currency`
  * `Status` (Pending, Success, Failed)
  * `CreatedAt`, `UpdatedAt`

---

## 5. Identity & Access Rules

### Entities: User, Role, Permission

* IdentityServer is the central authority for authentication and token issuance.
* Roles are tenant-scoped (e.g., each merchant manages their own staff roles).
* Platform-level roles (e.g., system admin) are global.

### Rules

* Default roles: `MerchantAdmin`, `MerchantStaff`, `Customer`, `PlatformAdmin`.
* Each API request must include a valid JWT token with `TenantId` claim.
* Access control:

  * Platform endpoints: restricted to `PlatformAdmin`.
  * Tenant resources: restricted to authenticated users with matching `TenantId`.
* Tokens must include role and permission claims for efficient policy checks.
* Password policies: minimum 8 chars, uppercase/lowercase/number/special.
* Email verification is mandatory for all merchant accounts before activation.

---

## 6. Platform Administration Rules

* The platform admin has visibility into all tenants (for support, billing, fraud detection).
* Platform settings include:

  * Global payment provider configuration (e.g., fallback keys).
  * Email/SMS notification templates.
  * System health metrics and service uptime logs.
* Platform may disable any tenant violating policy (fraud, abuse, etc.).

---

## 7. Cross-Domain Rules

### Audit & Logging

* All write operations must generate an audit log (who, what, when, before/after values where relevant).
* Sensitive data (passwords, tokens, card references) must be masked or excluded from logs.

### Notifications

* Email/SMS notifications must be queued via a background service (not synchronous with API requests).
* Notification templates must be tenant-customizable but controlled by admin approval for HTML safety.

### Events & Messaging

* Services communicate via domain events (in-process) and integration events (cross-service).
* All integration events must include `TenantId`, `EventId`, and `CorrelationId` for tracing.
* Event contracts versioned in `libs/shared/events`.

---

## 8. Data Integrity Rules

* Every entity must include `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`.
* Deletion is soft unless explicitly required by law or admin purge.
* Tenant deletion must cascade to dependent entities only when confirmed via admin process.
* Database constraints must enforce tenant isolation (`TenantId` in all tables).

---

## 9. Error Handling & Validation Rules

* API errors follow a unified format:

```
{
  "status": 400,
  "error": "BadRequest",
  "message": "Product name is required.",
  "traceId": "<guid>"
}
```

* Validation errors must include a list of field-specific errors.
* Business-rule violations raise domain-specific exceptions (e.g., `InvalidOrderStateException`).
* All exceptions must be logged centrally (Serilog + Application Insights/Prometheus metrics).

---

## 10. Security & Compliance

* PCI DSS handled by payment provider (tokenization & hosted checkout).
* The platform must never store or log full card details.
* All communication between services must use HTTPS/TLS.
* Use anti-forgery tokens for all form submissions in Angular.
* JWT tokens must be validated with IdentityServer public key.
* Multi-Factor Authentication (MFA) for platform admin and merchant admins (phase 2 feature).

---

## Next steps

Next document: `03_ROADMAP.md` — detailed development roadmap (timeline, phases, modules, and technology decisions).
