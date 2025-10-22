# Vendo - Multi-Tenant E-Commerce Platform

A comprehensive multi-tenant e-commerce platform built with **Microservices Architecture**, **Clean Architecture**, and **Micro-Frontend** pattern.

## Technology Stack

### Backend
- **.NET 9.0** - Latest version of ASP.NET Core
- **Microservices Architecture** - Independent, scalable services
- **Clean Architecture** - Domain-driven design with clear separation of concerns
- **IdentityServer** - Centralized authentication and authorization
- **Entity Framework Core** - ORM for database access
- **SQL Server** - Relational database with multi-tenant support
- **MediatR** - CQRS pattern implementation
- **FluentValidation** - Request validation
- **xUnit, Moq, FluentAssertions** - Testing framework

### Frontend
- **Angular 20** - Latest version with standalone components
- **Micro-Frontend Architecture** - Module Federation
- **Tailwind CSS** - Utility-first styling (NO component libraries)
- **TypeScript** - Type-safe development
- **Signal-based State Management** - Reactive data handling

## Project Structure

```
/
├── services/                      # Backend Microservices
│   ├── identity/                  # Authentication & Authorization (IdentityServer)
│   │   └── src/
│   │       ├── Domain/           # Entities, ValueObjects, Interfaces
│   │       ├── Application/      # Commands, Queries, DTOs, Validators
│   │       ├── Infrastructure/   # Data access, External services
│   │       └── Api/              # Controllers, Middleware
│   ├── catalog/                   # Product Management Service
│   ├── order/                     # Order Management Service
│   ├── payment/                   # Payment Integration Service
│   └── tenant-management/         # Tenant Provisioning Service
│
├── libs/                          # Shared Libraries
│   ├── shared/                    # Cross-cutting DTOs, Constants, Contracts
│   ├── infra/                     # DB context, Tenant abstractions, Repository base
│   └── tests/                     # Test utilities, Fixtures
│
├── frontend/                      # Frontend Applications
│   ├── shell-app/                 # Host Application (Module Federation)
│   ├── mfe-products/              # Products Micro-Frontend
│   ├── mfe-orders/                # Orders Micro-Frontend
│   ├── mfe-store/                 # Store Management Micro-Frontend
│   └── shared-lib/                # Shared Components & Utilities
│
├── docs/                          # Documentation
├── tools/                         # Scripts, Seed data
├── infra/                         # Infrastructure (Docker, K8s)
├── postman/                       # API Collection
└── .github/workflows/             # CI/CD Pipelines
```

## Architecture Principles

### Clean Architecture (Backend)
Each microservice follows Clean Architecture with four layers:
1. **Domain** - Business entities, value objects, domain events
2. **Application** - Use cases, CQRS handlers, DTOs, validators
3. **Infrastructure** - Data access, external integrations
4. **Api** - Controllers, middleware, dependency injection

### Micro-Frontend Architecture (Frontend)
- **Shell Application** - Hosts and orchestrates micro-frontends
- **Independent Micro-Frontends** - Developed, tested, and deployed separately
- **Module Federation** - Runtime composition using Webpack
- **Shared Library** - Common components and utilities

### Multi-Tenancy
- **Soft Multi-Tenancy** - Shared database with TenantId discriminator
- **Tenant Isolation** - Automatic filtering via middleware
- **Future-Ready** - Designed for migration to per-tenant databases

## Prerequisites

1. **.NET SDK 9.0+** - [Download](https://dotnet.microsoft.com/download)
2. **Node.js 22+ and npm** - [Download](https://nodejs.org/)
3. **Angular CLI 20+** - Install globally: `npm install -g @angular/cli`
4. **SQL Server** - Local instance or Docker container
5. **Docker Desktop** - For containerized development
6. **Visual Studio Code** - Recommended IDE

## Getting Started

### Backend Setup

1. **Restore and Build All Microservices:**
   ```bash
   # Identity Service
   cd services/identity
   dotnet restore
   dotnet build

   # Catalog Service
   cd ../catalog
   dotnet restore
   dotnet build

   # Order Service
   cd ../order
   dotnet restore
   dotnet build

   # Payment Service
   cd ../payment
   dotnet restore
   dotnet build

   # Tenant Management Service
   cd ../tenant-management
   dotnet restore
   dotnet build
   ```

2. **Run Individual Services:**
   ```bash
   # Run Identity Service (default: https://localhost:5001)
   cd services/identity/src/Api
   dotnet run

   # Run Catalog Service (default: https://localhost:5002)
   cd services/catalog/src/Api
   dotnet run
   ```

### Frontend Setup

1. **Shell Application:**
   ```bash
   cd frontend/shell-app
   npm install
   npm start
   # Opens at http://localhost:4200
   ```

2. **Micro-Frontends:**
   ```bash
   # Products MFE (port 4201)
   cd frontend/mfe-products
   npm install
   npm start

   # Orders MFE (port 4202)
   cd frontend/mfe-orders
   npm install
   npm start

   # Store MFE (port 4203)
   cd frontend/mfe-store
   npm install
   npm start
   ```

## Running Tests

### Backend Tests
```bash
# Run tests for all services
dotnet test

# Run tests for specific service
cd services/catalog
dotnet test

# With coverage
dotnet test /p:CollectCoverage=true
```

### Frontend Tests
```bash
# Run tests
cd frontend/shell-app
npm test

# Run e2e tests
npm run e2e
```

## Development Guidelines

### SOLID Principles
- **S**ingle Responsibility - Each class has one purpose
- **O**pen/Closed - Extend via interfaces, not modification
- **L**iskov Substitution - Derived classes don't break base behavior
- **I**nterface Segregation - Small, focused interfaces
- **D**ependency Inversion - Depend on abstractions

### Code Quality
- **TDD** - Write tests first
- **80% Code Coverage** - Minimum requirement
- **No Logic in Controllers** - Keep them thin
- **Clean Architecture** - Strict layer separation
- **Code Reviews** - Required for all PRs

### Frontend Rules
- **NO UI Libraries** - No Angular Material, PrimeNG, etc.
- **Tailwind CSS Only** - For all styling
- **NgModule-based** - Not standalone components
- **Signal-based State** - Reactive data management

### API Standards
- **RESTful** - Follow REST conventions
- **Versioned** - `/api/v1/...`
- **Paginated** - List endpoints support pagination
- **Standardized Errors** - RFC 7807 Problem Details

## Tailwind CSS Setup

Tailwind is already configured in all Angular projects:
- Configuration: `tailwind.config.js`
- Directives added to: `src/styles.css`
- No additional setup needed

The CSS warnings about `@tailwind` directives are normal and can be ignored.

## CI/CD

GitHub Actions workflows:
- **ci.yml** - Build, test, lint on every PR
- **integration.yml** - Integration tests with Docker
- **publish.yml** - Build and publish on tag

## Documentation

- [Technical Decisions](./04_TECH_DECISIONS.md) - Architecture and tech stack
- [Setup Steps](./01_STEPS.md) - Detailed setup and conventions
- [Business Rules](./02_BUSINESS_RULES.md) - Domain rules and constraints
- [Code Conventions](./05_CODE_CONVENTIONS.md) - Coding standards

## Security

- **Hosted Checkout** - Never store card data
- **IdentityServer** - Centralized authentication
- **OAuth2/OIDC** - Industry-standard protocols
- **Environment Variables** - No secrets in code
- **Webhook Verification** - All payment webhooks verified

## Contributing

1. Create feature branch from `development`
2. Follow coding standards and SOLID principles
3. Write tests (TDD approach)
4. Update documentation
5. Submit PR with checklist completed
6. Ensure CI passes

## Version Information

- **.NET:** 9.0.301
- **Angular:** 20.0.4
- **Node.js:** 22.17.0
- **npm:** 10.9.2

---

**Last Updated:** 2025-10-20

For questions or issues, please refer to the documentation in the `/docs` folder.
