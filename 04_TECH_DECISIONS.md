# Technical Decisions and Enforcement Guide

This document defines all major technical decisions, architectural rules, and enforcement practices. It serves as the **technical constitution** for this project — every developer and AI agent must adhere to these principles.

---

## 1. Core Technologies

| Layer     | Technology                                | Purpose                                                                          |
| --------- | ----------------------------------------- | -------------------------------------------------------------------------------- |
| Frontend  | **Angular 17+**, TypeScript, Tailwind CSS | Reactive, component-based UI (no UI frameworks like Angular Material or PrimeNG) |
| Backend   | **ASP.NET Core 9+**                       | REST APIs, microservices, domain logic                                           |
| Auth      | **IdentityServer**                        | Centralized authentication and authorization (OpenID Connect + OAuth2)           |
| Database  | **SQL Server** (EF Core)                  | Relational data store with multi-tenant awareness                                |
| Messaging | (Future) RabbitMQ / Azure Service Bus     | For asynchronous operations and decoupled services                               |
| CI/CD     | **GitHub Actions**                        | Automated build → test → deploy pipeline                                         |
| Testing   | **xUnit**, **FluentAssertions**, **Moq**  | Test-driven development enforcement                                              |
| Docs      | Markdown (`/docs`), Postman collection    | Always up-to-date technical and API documentation                                |

---

## 2. Architecture

### Clean Architecture Layers

```
/src
 ├── Core            → Entities, ValueObjects, Interfaces, Domain Events
 ├── Application     → CQRS Handlers, DTOs, Validators, Contracts
 ├── Infrastructure  → EF Core, Repositories, IdentityServer Integration
 ├── WebAPI          → Controllers, Middleware, Dependency Injection setup
 ├── Frontend        → Angular app (separate project)
```

### Enforced Rules

* Controllers → **thin**; no business logic.
* Handlers → must live in the `Application` layer.
* Domain models → **pure**, with no dependencies on EF Core or Infrastructure.
* No service should directly depend on another service’s internal models — use shared contracts or DTO packages.
* All dependencies must follow **Dependency Inversion Principle**.

### CQRS Enforcement

* Every command and query must have a distinct **Handler**.
* Use **MediatR** for dispatching commands/queries.
* Validation is performed using **FluentValidation** on each request.

---

## 3. SOLID Enforcement

| Principle                     | Enforcement                                                                        |
| ----------------------------- | ---------------------------------------------------------------------------------- |
| **S** – Single Responsibility | Each class serves one purpose only (e.g., ProductValidator only validates).        |
| **O** – Open/Closed           | Extend via new implementations, not modification. Use interfaces and abstractions. |
| **L** – Liskov Substitution   | Derived classes must not break base behavior; avoid over-inheritance.              |
| **I** – Interface Segregation | Prefer small, focused interfaces (e.g., `IEmailSender`, `IPaymentProvider`).       |
| **D** – Dependency Inversion  | Always depend on abstractions; register them in DI container.                      |

---

## 4. Cross-Cutting Concerns

### Logging

* Use **Serilog** (structured JSON logs).
* Logging pipeline must include correlation ID and tenant ID.
* No `Console.WriteLine()` allowed.

### Validation

* All requests validated with **FluentValidation**.
* Validation happens before handler execution.

### Error Handling

* Use centralized middleware for exception handling.
* Never expose stack traces in API responses.
* Return problem details in standardized format (RFC 7807).

### Configuration

* Use `appsettings.{Environment}.json`.
* Secrets managed through environment variables or secret store.
* Never commit secrets or tokens to repo.

---

## 5. Database and Multi-Tenancy

* Use a **TenantId** column in all tables (soft multi-tenancy).
* Abstract tenant resolution via middleware and `ITenantContext` service.
* All EF Core queries must filter automatically by current tenant.
* Future migration to per-tenant DB must require minimal refactoring.

---

## 6. API Standards

* RESTful conventions:

  * `GET /api/products`
  * `POST /api/products`
  * `PUT /api/products/{id}`
  * `DELETE /api/products/{id}`
* Return `IActionResult` from controllers.
* Always include versioning (e.g., `/api/v1/products`).
* Use pagination, filtering, and sorting for list endpoints.
* API responses should include `traceId`, `tenantId`, and standardized error messages.

---

## 7. Frontend Rules (Angular)

* No external UI libraries (e.g., Material, PrimeNG).
* Tailwind used for all styling.
* Use **standalone components** and **signal-based state management**.
* Directory structure:

  ```
  /frontend/src/app
   ├── core          → Services, interceptors, auth guards
   ├── features      → Modules (products, orders, store)
   ├── shared        → Reusable components, directives
   ├── state         → NgRx or signals for global store
  ```
* Each feature folder must have `README.md` describing its purpose.
* Unit tests required for all services and components.

---

## 8. CI/CD Enforcement (GitHub Actions)

### Workflow Stages

1. **Build:** Restore NuGet & npm packages, run build for backend & frontend.
2. **Test:** Execute unit tests for all projects. Build fails on any test failure.
3. **Deploy (Staging):** Deploys backend + frontend artifacts using environment secrets.

### Policies

* All pull requests must pass build + test before merging.
* Auto-update Postman collection after successful API deployment.
* Auto-commit updated documentation (README + changelog).

---

## 9. Testing & Quality Gates

* Enforce **TDD** across all features.
* Minimum coverage: **80% unit test coverage**.
* Each service must have tests for: success path, validation failure, and exception cases.
* Integration tests run on local SQL Server instance.
* Code review checklist:

  * ✅ SOLID compliance
  * ✅ No static dependencies
  * ✅ No logic in controllers
  * ✅ Tests updated
  * ✅ Documentation updated

---

## 10. Coding Best Practices

* Use `async/await` for all I/O operations.
* Avoid `.Result` and `.Wait()` calls.
* Use `IOptions<T>` for configuration.
* No circular dependencies between layers.
* Keep all classes under 200 lines where possible.
* Prefer immutability for DTOs and domain entities.

---

## 11. Documentation Rules

* `/docs` folder must always reflect current architecture.
* Each module has its own subfolder under `/docs/modules`.
* Update Postman collection per endpoint addition.
* `README.md` must summarize project state after each sprint.
* ADRs (Architectural Decision Records) required for all non-trivial design choices.

---

### 🏁 Summary

This document enforces consistency, scalability, and quality. Every contributor and AI agent must follow these rules strictly to maintain the system’s architectural integrity, performance, and long-term maintainability.
