# 01_STEPS.md

## Purpose

This document defines the initial **project setup steps**, repository layout, development conventions, and operational rules that the team (initially a single developer) will follow. These steps are written as a contract for human developers and AI agents that will read the repository docs and assist development.

## Scope

Covers: local developer onboarding, repository (monorepo) structure, branch and commit conventions, CI/CD basics (GitHub Actions), testing rules (TDD), documentation update rules (README, docs, Postman collection), and security/payment principles (hosted checkout/tokenization).

---

## Quick assumptions

* Stack: Angular (frontend), ASP.NET Core (backend microservices), SQL Server.
* Auth: IdentityServer (centralized OpenID Connect / OAuth2 provider).
* Architecture: Microservices, Clean Architecture, SOLID principles in all code.
* Monorepo: single repository containing all services, libs, and apps.
* UI: Tailwind CSS only — no component libraries (no PrimeNG / Angular Material).
* Payments: Hosted checkout / tokenized flow (do NOT store card data).
* CI: GitHub Actions.

---

## Onboarding / Developer Prerequisites

1. Install .NET (matching project SDK version).
2. Install Node.js (LTS) + npm or pnpm (project will standardize on one — `pnpm` is recommended).
3. Install Angular CLI globally if needed: `npm i -g @angular/cli`.
4. Install SQL Server locally or use Docker image for development.
5. Install Docker Desktop (for running dependent services locally).
6. VS Code or preferred IDE with recommended extensions (C#, Angular, Tailwind IntelliSense).

---

## Repository layout (monorepo suggested structure)

```
/ (root)
  README.md
  docs/                  # human readable docs (architectural notes, patterns)
  tools/                 # scripts, infra helpers, seed data
  apps/
    admin-portal/        # Angular admin/merchant dashboard
    storefront/          # Optional storefront project (Angular)
  services/
    gateway/             # API Gateway / BFF (if used)
    identity/            # IdentityServer / Auth service
    catalog/             # product service (Clean Arch)
    order/               # order service
    payment/             # integration service (non-card storing orchestration)
    tenant-management/   # tenant abstraction / provisioning
  libs/
    shared/              # cross-cutting DTOs, constants
    infra/               # db context, tenant abstractions, repo base
    tests/               # test utilities, fixtures
  infra/
    docker-compose.yml
    k8s/                 # optional (kept infra-agnostic)
  .github/
    workflows/           # GitHub Actions pipelines
  postman/
    collection.json
```

> Notes:
>
> * Each `service` follows Clean Architecture (separate `Domain`, `Application`, `Infrastructure`, `Api` projects within its folder).
> * Keep cross-service domain coupling minimal — use minimal, well-defined contracts (DTOs) published in `libs/shared`.

---

## Initial repo / project setup steps (ordered)

1. Create repository and enable branch protections for `main` (require PR + approvals).
2. Create initial monorepo skeleton with folders above and a minimal README.md.
3. Add EditorConfig and `.gitattributes`.
4. Add `.github/workflows/ci.yml` (basic build + test matrix) — see `CI` section.
5. Add `apps/admin-portal` (Angular workspace) with Tailwind configured.
6. Add `services/identity` (IdentityServer project) with minimal seed users & clients recorded in `tools/seeds`.
7. Add `services/catalog` and `services/order` skeletal solutions implementing Clean Architecture patterns.
8. Add `libs/shared` for DTOs and `libs/infra` for tenant abstractions.
9. Add `postman/collection.json` (empty template) and include a Postman update guideline.
10. Add `docs/CONTRIBUTING.md`, `docs/ARCHITECTURE.md`, `docs/CODE_STYLE.md`.

---

## Tenant model & database strategy (abstraction guidance)

**Primary choice:** Shared single database with tenant discriminator (column `TenantId`) for MVP.

**Design requirement:** Abstract the tenant data access so switching to one-DB-per-tenant later is possible with minimal code changes.

### Implementation constraints (from day 1)

* All EF Core `DbContext` access must go through an abstraction layer that accepts an `ITenantContext` or `TenantInfo` object.
* `libs/infra` provides `ITenantProvider`, `ITenantResolver`, and `ITenantConnectionFactory` interfaces.
* Default implementation for MVP: `TenantConnectionFactory` returns the shared DB connection string; it also provides `TenantId` for queries.
* Later swap: implement `PerTenantConnectionFactory` which returns per-tenant connection strings (no other service changes required).

---

## Payments (hosted checkout - tokenized)

* Integrate with provider using **hosted checkout pages** or **client-side tokenization** (Stripe Checkout, PayPal Hosted, Paymob hosted, etc.).
* Backend `payment` service will store only **payment identifiers**, statuses, and provider metadata — **never raw card details**.
* Design the payment flow as an orchestration: `Order -> Create Payment Session via Payment Service -> Return checkout URL to frontend -> Frontend redirects -> Payment provider notifies via webhook -> Payment Service validates and updates Order`.
* Document provider-specific Webhook verification steps in `docs/payments.md`.

---

## Identity & Authentication

* IdentityServer will be the central IdP. Keep it limited to authentication + authorization concerns.
* Users: platform admins, merchants, merchant-staff, and customers (if required).
* Use ASP.NET Core Identity for user storage and IdentityServer on top for token issuance.
* Use `Client Credentials` for server-to-server auth and `Authorization Code` with PKCE for SPAs (Angular).

---

## TDD & Testing conventions

* Tests are required for all new features. Follow TDD: write failing unit tests first.
* Test projects per service under `services/<service>/tests`.
* Test types:

  * Unit tests (xUnit, FluentAssertions, Moq)
  * Integration tests (in-memory or test DB - use SQL Server container for closer parity)
  * Contract tests for service boundaries (e.g., Pacts or simple contract checks)
* Coverage: aim for high coverage on domain & application layers; controllers and infrastructure lower priority but still covered.

---

## Code style, SOLID & Clean Architecture enforcement

* Every pull request must include:

  * Unit tests for new logic.
  * A short note how SOLID principles were applied.
* Use static analysis: SonarCloud / dotnet-format / Roslyn analyzers in CI.
* Do not mix infrastructure concerns into domain layer. Keep DTOs separate from Entities.

---

## Tailwind & Frontend rules

* Use Tailwind for styling. No external UI libs. Build a small design tokens library in `apps/admin-portal/src/styles/tokens`.
* Components should be atomic and documented using Storybook or a lightweight component catalog (optional but recommended).
* Accessibility: basic WCAG checks for forms and checkout flows.

---

## Git & Branching

* Branching: **trunk-based** with feature branches. `main` is always ready to release.
* PR policy: require 1 approval for feature PRs (increase later). CI must pass.
* Commit messages: Conventional Commits (`feat:`, `fix:`, `chore:`, `docs:`).

---

## CI (GitHub Actions) - required pipelines

1. `ci.yml` (on PR): restore, build, run unit tests, run linters/formatters, run `dotnet format --verify-no-changes`.
2. `integration.yml` (scheduled or manual): run integration tests using Docker Compose with SQL Server + dependent services.
3. `publish.yml` (on tag): build images (Docker), push to container registry (configurable via secrets), and generate release artifacts.

> Keep all secrets in GitHub Secrets. Do not commit secrets to the repo.

---

## Documentation rules (README + docs + Postman)

* **README.md** at repo root must always have:

  * Project summary
  * Stack and architecture overview
  * How to run locally (short commands)
  * How to run tests
  * How to update Postman collection
* **docs/** contains living docs. Each doc must start with `Last updated: YYYY-MM-DD` and an author line.
* **Postman**: After any API change, update `postman/collection.json` and update `docs/POSTMAN_UPDATE.md` with the change summary. Include the Postman collection version in README.

---

## Postman collection update policy

* When an API endpoint changes (signature, path, body, auth), update Postman collection immediately before merging the PR.
* Postman collection must include sample env variables for local development (use placeholder values). The actual keys remain in developer environment files.

---

## Code review checklist (required in PR description)

* [ ] All new code follows SOLID and Clean Arch.
* [ ] Unit tests written and passing.
* [ ] No secrets pushed.
* [ ] README/docs updated if public contract changed.
* [ ] Postman collection updated if API changed.
* [ ] CI green.

---

## README update template (include in PR if changed)

```
### Changelog - YYYY-MM-DD
- Short description of change
- Files updated: docs/..., postman/collection.json
```

---

## Security & secret management

* Use environment variables + Azure Key Vault / AWS Secrets Manager (abstract in `libs/infra/secrets`) for production secrets.
* Locally use `.env.local` files excluded from git. Provide `.env.example` with placeholders.
* Always require webhook signature verification for payment providers.

---

## Next steps (what I will produce next)

1. `02_BUSINESS_RULES.md` (business rules and domain invariants) — next file.
2. `03_ROADMAP.md` (development roadmap with timeline, modules, tech decisions) — after business rules.

---

**If anything here is incorrect or you want adjustments, tell me now.**
Otherwise I'll create `02_BUSINESS_RULES.md` next.
