# Development Roadmap

## Phase 1 – Foundation (≈ 3 Weeks)

### 🎯 Objectives

Establish the core technical foundation, ensuring architecture, security, and automation are in place before functional development begins.

### 🧩 Deliverables

* Monorepo structure (Angular + ASP.NET Core + IdentityServer)
* Clean Architecture and SOLID principles enforcement templates
* Shared libraries (logging, error handling, DTOs, API contracts)
* Tenant abstraction (single DB, extensible for future per-tenant DB)
* IdentityServer setup (multi-tenant ready, OpenID Connect + OAuth2)
* CI/CD pipeline using GitHub Actions: **build → test → deploy to staging**
* Tailwind CSS integration for Angular UI styling
* Base README and Documentation files (auto-updated per commit)
* Postman collection initialization for shared API documentation

### 🧠 Tech Decisions

* ASP.NET Core WebAPI (clean architecture layers)
* Angular 17+ standalone structure, no component libraries (only Tailwind)
* SQL Server shared DB (EF Core with tenant-aware context)
* IdentityServer for authentication/authorization
* GitHub Actions pipeline:

  * **Build**: dotnet restore/build, Angular build
  * **Test**: run .NET unit tests (xUnit), Angular Karma tests
  * **Deploy**: auto-deploy to staging environment via environment secrets

### 📘 Documentation & Testing

* Every project must have a `/docs` subfolder (architecture, APIs, workflows)
* TDD enforced: each feature must begin with tests before implementation
* Update README after each milestone or architectural change

---

## Phase 2 – MVP (≈ 2 Months)

### 🎯 Objectives

Deliver the core business capabilities for a functional commerce platform.

### 🧩 Deliverables

* **Store Management**

  * Create and configure stores (name, domain, logo, theme)
  * Merchant admin dashboard
* **Product Module**

  * Product CRUD with categories, inventory, and variants
  * Image management (abstracted storage provider)
* **Checkout Flow**

  * Cart and order placement
  * Integration with one payment provider using **hosted checkout (tokenized)**
  * Secure callback handling and order status tracking
* **Order Management**

  * Order listing, filtering, and status updates
* **Merchant Admin Panel** (Angular)

  * Dashboard, analytics placeholder, store settings
* **Tenant Isolation Middleware**

  * Identify tenant by domain/subdomain
  * Filter queries and services per tenant
* **Enhanced CI/CD**

  * Auto run tests, build Angular app, and deploy to staging after pull requests
  * Automatic README and Postman collection update via GitHub Actions

### 🧠 Tech Decisions

* Communication between microservices via REST for MVP
* Service boundaries:

  * Auth Service (IdentityServer)
  * Store Service
  * Product Service
  * Order Service
  * Payment Gateway Integration Service
* Common NuGet and npm packages for DTOs and shared contracts

### 📘 Documentation & Testing

* Expand Postman collection with each endpoint release
* Unit + Integration tests per module
* README updated automatically via GitHub workflow
* Architectural Decision Records (ADRs) under `/docs/decisions`

---

## Phase 3 – Scale (Ongoing)

### 🎯 Objectives

Refactor for scalability, performance, extensibility, and readiness for production-grade multi-tenancy.

### 🧩 Deliverables

* Tenant-aware database strategy (option for per-tenant DBs)
* Distributed caching (Redis or SQL-based cache)
* Message Queue integration (e.g., RabbitMQ or Azure Service Bus)
* Payment provider modularization (allow adding more providers)
* Analytics module (store sales, traffic, customer reports)
* Plugin system for store extensibility (future marketplace support)
* API Gateway or BFF layer for consolidated access
* Optional deployment pipelines to cloud environments (abstracted hosting)

### 🧠 Tech Decisions

* Consider containerization (Docker Compose or Kubernetes for multi-service orchestration)
* Infrastructure as Code (Terraform or Bicep templates for future cloud setup)
* Multi-environment pipelines in GitHub Actions (staging → production)

### 📘 Documentation & Testing

* Load & performance testing integration
* Deployment & scaling runbooks
* Developer onboarding guide
* Maintain changelog and ADRs regularly

---

### 🏁 Summary

This roadmap ensures disciplined, test-driven, and SOLID-aligned growth from prototype to scalable platform. Each phase builds on the last, ensuring security, maintainability, and clean architecture while allowing flexibility for future scaling and infrastructure evolution.
