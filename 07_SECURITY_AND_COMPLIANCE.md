# 07_SECURITY_AND_COMPLIANCE.md

## Purpose

This document defines the security and compliance controls the platform must implement. It is tailored to your chosen architecture (IdentityServer, hosted/tokenized payments, multi-tenant microservices, GitHub Actions CI) and is written as an enforceable checklist for developers and AI agents.

All controls here are **mandatory** unless explicitly scoped to "phase 2/3". Follow these rules to protect customer data, reduce legal risk, and prepare for audits.

---

## 1. High-level Principles

* **Never store raw card data**. Use hosted checkout/tokenization for all payment flows.
* **Least privilege**: services and humans must have the minimal privileges required.
* **Defense in depth**: multiple layers of protection (network, app, data).
* **Auditability**: all security-relevant actions must be logged and traceable (who, what, when, tenantId).
* **Separation of duties**: platform admin capabilities are segregated from tenant admin capabilities.

---

## 2. Payments: Tokenized / Hosted Checkout (MVP & beyond)

* Use a provider that supports hosted checkout sessions or client-side tokenization (Stripe Checkout, PayPal Hosted, Paymob, etc.).
* Flow enforcement:

  1. Backend `PaymentService` creates a hosted checkout session or issues a client token.
  2. Frontend redirects customer to provider's hosted page or uses the provider SDK to collect card tokens.
  3. Provider returns a `ProviderTransactionId` and calls the configured webhook on completion.
  4. Webhook handler verifies signature, validates amount/order id, and marks the order `Paid`.
* **Do not** accept raw card numbers or CVV in any API or logs.
* Store only provider tokens, transaction IDs, and minimal metadata required for reconciliation.

**Webhook security:**

* Verify webhook signatures per provider docs.
* Implement idempotency (reject duplicate event processing by `EventId` or `ProviderTransactionId`).
* Log webhook payloads (masked) for forensic analysis.

**PCI DSS note:**

* By using hosted checkout/tokenization and never storing card PAN/CVV, most PCI DSS scope is transferred to the provider. However, you still must maintain secure integration, logging, and incident response. Document your cardholder data flow for audit readiness.

---

## 3. Identity, Authentication & Authorization

* IdentityServer is the central IdP.
* Use **OpenID Connect** flows:

  * SPAs: Authorization Code with PKCE
  * Server-to-server: Client Credentials
* Enforce **strong password policy** (min 8 chars, mixed-case, number, special) and **password hashing** (ASP.NET Core Identity defaults are acceptable).
* **MFA**: require for PlatformAdmin and MerchantAdmin (Phase 2/3 rollout).
* Token best practices:

  * Use short-lived access tokens and refresh tokens where necessary.
  * Validate tokens using IdentityServer public keys (JWKs).
  * Revoke refresh tokens on critical events (password change, staff removal).
* Tenant claim: include `tenantId` claim in access tokens for downstream authorization checks.

---

## 4. Network & Transport Security

* Enforce HTTPS/TLS for all network traffic (internal and external).
* Use TLS 1.2+; disable weak ciphers.
* Service-to-service communication must be mutually authenticated where possible (mTLS or secure token exchange).
* Use API Gateway for ingress controls (rate limiting, IP filtering) in Phase 3.

---

## 5. Secrets & Configuration Management

* Never commit secrets to source control.
* Local development: use `.env.local` or local secret files excluded from VCS and a provided `.env.example`.
* CI/CD: store secrets in GitHub Secrets and restrict access to necessary workflows.
* Production: use a managed secret store (Azure Key Vault, AWS Secrets Manager) — abstract via `libs/infra/secrets`.
* Key rotation policy: rotate keys and credentials every 90 days or sooner for sensitive keys. Automate rotation where possible.

---

## 6. Data Protection & Storage

* Encrypt sensitive fields at rest where required (e.g., internal tokens). Use provider-managed encryption (KMS).
* Use column-level encryption or tokenization for any internal sensitive identifiers if needed.
* Backups: encrypted backups, access-controlled, retention policy documented.
* Data retention: define retention periods for tenant data and payment metadata in `docs/data-retention.md`.

---

## 7. Tenant Isolation & Multi-Tenancy Security

* Enforce tenant scoping on every request using `ITenantContext`.
* DB-level: include `TenantId` foreign key in all tables and apply EF Core query filters to enforce isolation.
* Prevent horizontal privilege escalation: verify `tenantId` claim matches requested resource.
* Soft-delete patterns for tenant data; ensure purging requires admin approval and is auditable.

---

## 8. Logging, Monitoring & Alerting

* Use Serilog for structured logs with these required fields: `timestamp`, `level`, `message`, `service`, `tenantId`, `userId`, `correlationId`, `traceId`.
* Do not log secrets or full PII; mask or redact sensitive values.
* Centralize logs (Application Insights, ELK, or other). Configure retention and access controls.
* Monitoring: integrate Prometheus + Grafana or a managed alternative for metrics and health checks.
* Alerts:

  * Payment webhook failures
  * High rate of failed logins (possible brute-force)
  * Elevated error rates or latency
  * Resource saturation (CPU, memory)

---

## 9. Secure Development Lifecycle & CI/CD

* Enforce static analysis and SAST tools in CI (e.g., SonarQube, dotnet analyzers).
* Add dependency scanning (SCA) to detect vulnerable packages (Dependabot, GitHub Code Scanning, or Snyk).
* Run automated tests and security checks on PRs.
* Build artifacts must be reproducible and signed in production pipelines.

---

## 10. Input Validation & Output Encoding

* Validate all input at API boundary using **FluentValidation**.
* Use parameterized queries/EF Core to prevent SQL injection.
* Apply output encoding on HTML or template rendering to prevent XSS.
* Use CSP headers and secure cookies (HttpOnly, Secure, SameSite=strict where applicable).

---

## 11. CORS, Rate Limiting & DDoS Protection

* Configure strict CORS policies allowing known origins only.
* Rate limit critical endpoints (login, checkout) and throttle per IP/tenant.
* In production, use WAF or cloud provider DDoS protection features.

---

## 12. Vulnerability Management

* Subscribe to vendor security advisories for all critical dependencies.
* Patch critical vulnerabilities within 7 days, high within 30 days.
* Maintain a dependency inventory and upgrade plan.

---

## 13. Incident Response & Breach Procedures

* Maintain an incident response plan and contact list in `docs/incident-response.md`.
* On suspected data breach:

  1. Contain the incident (isolate affected services).
  2. Revoke compromised credentials and rotate keys.
  3. Notify platform admin and affected tenants per legal requirements.
  4. Preserve logs and evidence for post-incident analysis.
* Record post-mortem and remediation steps.

---

## 14. Compliance & Audit Readiness

* Keep documentation of cardholder data flow and hosted checkout integration for PCI auditors.
* Maintain change logs, ADRs, and access logs for auditability.
* Prepare for third-party audits if onboarding enterprise merchants.
* Consider privacy regulations (GDPR) if targeting EU customers — implement data subject requests, data portability, and erasure workflows.

---

## 15. Developer Security Checklist (pre-merge)

* [ ] No secrets in code or logs
* [ ] All new endpoints validated and unit-tested
* [ ] Webhooks verify provider signature
* [ ] Token-based auth enforced; tenant claim validated
* [ ] Linting, SAST, and SCA checks pass in CI
* [ ] Postman collection and docs updated if API changed

---

## 16. Operational Next Steps

1. Create `docs/data-retention.md` describing retention & purge policy.
2. Implement webhook verification templates for each provider in `docs/payments.md`.
3. Add SCA and SAST steps to GitHub Actions workflows.
4. Define backup and restore runbooks.

---

## 17. Summary

By following this document you will:

* Keep PCI scope minimal by using hosted checkout/tokenization.
* Protect tenant separation and data confidentiality.
* Maintain audit-ready controls and operational readiness for incidents.

**All AI agents and developers must reference this document before implementing payment or security-related code.**
