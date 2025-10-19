# 05_CODE_CONVENTIONS.md

## 1. Purpose

This document defines consistent naming, structure, commit, and review standards across backend, frontend, and documentation. It ensures readability, maintainability, and enforces SOLID and Clean Architecture discipline.

---

## 2. Naming Conventions

### 2.1 C# (.NET)

| Entity          | Convention      | Example                              |
| --------------- | --------------- | ------------------------------------ |
| Classes         | PascalCase      | `OrderService`, `ProductController`  |
| Interfaces      | Prefix with `I` | `IOrderRepository`                   |
| Methods         | PascalCase      | `CreateOrder()`, `ValidatePayment()` |
| Private fields  | `_camelCase`    | `_logger`, `_unitOfWork`             |
| Local variables | camelCase       | `orderItems`, `tenantId`             |
| Constants       | ALL_CAPS        | `MAX_ORDER_ITEMS`                    |
| Namespaces      | PascalCase      | `ShopifyClone.Application.Orders`    |

### 2.2 Angular / TypeScript

| Entity      | Convention                    | Example                   |
| ----------- | ----------------------------- | ------------------------- |
| Components  | PascalCase                    | `StoreDashboardComponent` |
| Services    | PascalCase + `Service` suffix | `CheckoutService`         |
| Directories | kebab-case                    | `store-dashboard/`        |
| Variables   | camelCase                     | `storeName`, `isVisible`  |
| Constants   | ALL_CAPS                      | `DEFAULT_CURRENCY`        |
| Interfaces  | Prefix with `I`               | `IProduct`, `IOrder`      |
| Files       | kebab-case                    | `checkout.service.ts`     |

### 2.3 SQL

| Entity            | Convention            | Example                |
| ----------------- | --------------------- | ---------------------- |
| Tables            | PascalCase (singular) | `Store`, `Order`       |
| Columns           | PascalCase            | `StoreId`, `CreatedAt` |
| Stored Procedures | PascalCase with Verb  | `GetOrdersByTenant`    |

---

## 3. Folder and Project Structure

### Backend (.NET)

```
/src
  /Core                → Domain entities, value objects, interfaces
  /Application         → CQRS handlers, DTOs, validators, services
  /Infrastructure      → EF Core, IdentityServer, integrations
  /WebApi              → Controllers, filters, DI setup
  /Tests               → Unit, Integration, and E2E tests
/docs                  → Markdown documentation
/postman               → Postman collections
```

### Frontend (Angular)

```
/src
  /app
    /core              → Global services, guards, interceptors
    /shared            → Common components, pipes, directives
    /modules           → Feature modules (e.g., products, checkout)
    /environments      → Environment configurations
/docs                  → Markdown documentation
```

---

## 4. Commit Message Convention

Use **Conventional Commits** format:

```
<type>(<scope>): <summary>
```

### Common Types:

* `feat` — new feature
* `fix` — bug fix
* `docs` — documentation only changes
* `style` — formatting, no code logic change
* `refactor` — restructure without behavior change
* `test` — add or modify tests
* `build` — CI/CD or build system changes
* `chore` — maintenance, dependencies, cleanup

**Examples:**

```
feat(auth): implement tokenized login flow
fix(order): correct subtotal calculation
refactor(core): extract repository interface
```

---

## 5. Branch Naming Convention

```
<type>/<short-description>
```

**Examples:**

```
feat/checkout-module
fix/payment-provider-timeout
refactor/product-service
```

* Branch from `develop`
* PR targets `develop`
* `main` used only for stable releases

---

## 6. Pull Request (PR) Rules

* Include a **clear title** and **description**
* Reference related issue(s) if applicable
* Update **README**, **docs**, and **Postman collection** if functionality changes
* Ensure all tests pass
* Peer review required before merging
* No direct push to `main` or `develop`

---

## 7. Code Style & Formatting

### C#

* Use `var` when type is evident
* Use expression-bodied members for short logic
* Apply `async`/`await` with cancellation tokens
* Depend on abstractions (DIP compliance)
* Never instantiate services directly with `new`

### Angular / TypeScript

* Enforce ESLint + Prettier formatting
* Components ≤ 300 lines; extract logic into services
* Use standalone components (no shared modules)
* Keep templates clean — minimal logic, no duplication

### Markdown & Documentation

* Use meaningful headers, tables, and fenced code blocks
* Write in **formal English**
* Update docs alongside any architectural or workflow changes

---

## 8. CI/CD Enforcement Examples

* CI validates commit message format (Conventional Commits)
* Linting & tests must pass before merge
* Documentation check: reject PR if affected area lacks updates
* Manual review required for SOLID and Clean Architecture compliance

---

## 9. Example Developer Workflow

```
git checkout develop
git checkout -b feat/store-creation
# implement feature
# run tests and lint
git commit -m "feat(store): add create store endpoint"
git push origin feat/store-creation
# open PR → develop
```

---

## 10. Summary

Every contribution must:

* Adhere to naming and structure rules
* Maintain SOLID and Clean Architecture
* Include tests, updated docs, and passing CI
* Follow Conventional Commits and PR workflow

> **This file acts as the permanent coding contract for all contributors and AI agents.**
