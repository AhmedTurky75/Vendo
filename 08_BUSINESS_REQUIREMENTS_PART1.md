# Business Requirements Document - Part 1
**Vendo Platform - Multi-Tenant E-Commerce Solution**

**Document Version:** 1.0
**Date:** 2025-10-19
**Author:** Business Analyst
**Status:** Draft for Review

---

## Document Control

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-10-19 | Business Analyst | Initial document - Part 1 |

**Cross-References:**
- 01_STEPS.md - Technical setup and conventions
- 02_BUSINESS_RULES.md - Domain rules and invariants
- 03_DEVELOPMENT_ROADMAP.md - Implementation timeline
- 04_TECH_DECISIONS.md - Architecture and technology standards
- 05_CODE_CONVENTIONS.md - Code quality standards
- 06_TESTING_STRATEGY.md - Quality assurance approach
- 07_SECURITY_AND_COMPLIANCE.md - Security requirements

---

# Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Stakeholder Analysis & Personas](#2-stakeholder-analysis--personas)
3. [Functional Requirements by Domain](#3-functional-requirements-by-domain)
4. [Key Business Questions & Answers](#4-key-business-questions--answers)

---

# 1. Executive Summary

## 1.1 Vision Statement

Vendo is a modern, multi-tenant e-commerce platform designed to empower small and medium-sized businesses (SMBs) to launch and operate online stores with enterprise-grade capabilities. Built on a foundation of clean architecture, security-first design, and developer-friendly extensibility, Vendo aims to provide a competitive alternative to Shopify and WooCommerce while maintaining architectural flexibility for future scale.

**Core Vision:** Enable any SMB to go from idea to operational online store in under 30 minutes, with minimal technical expertise, while maintaining full data ownership and customization capabilities.

## 1.2 Business Objectives

### Primary Objectives (MVP - 6 Months)

1. **Time to Market:** Deliver a functional MVP within 3-4 months that allows merchants to:
   - Create and configure a branded online store
   - List products with variants and manage inventory
   - Accept payments through hosted checkout (PCI-compliant)
   - Process and fulfill customer orders

2. **Market Validation:** Onboard 10-20 pilot merchants within 6 months of MVP launch to validate:
   - Core feature set completeness
   - User experience and merchant satisfaction
   - Technical stability and performance
   - Pricing model viability

3. **Technical Foundation:** Establish a scalable, secure, and maintainable codebase that:
   - Supports multi-tenancy from day one
   - Enables rapid feature iteration
   - Maintains 80%+ test coverage
   - Achieves 99.5% uptime SLA

### Secondary Objectives (12-24 Months)

4. **Revenue Growth:** Achieve $10K MRR within 12 months through tiered subscription model
5. **Market Expansion:** Support international merchants with multi-currency and localization
6. **Ecosystem Development:** Enable third-party developers to build extensions and integrations
7. **Scale & Performance:** Support 1,000+ active tenants with sub-200ms API response times

## 1.3 Target Market

### Primary Market (MVP Focus)

**Small to Medium Businesses (SMBs) - US Market**

- **Size:** 1-50 employees, $100K-$5M annual revenue
- **Industries:**
  - Apparel and fashion
  - Handmade/artisan goods
  - Health and beauty products
  - Digital products and services
  - Niche consumer electronics

- **Characteristics:**
  - Currently selling on marketplaces (Etsy, Amazon, eBay) seeking own brand presence
  - Limited technical expertise (non-developers)
  - Budget-conscious but willing to invest in growth tools
  - Value brand control and customer data ownership
  - Need 50-500 products in catalog initially

- **Geographic Focus:** United States (initial MVP)
  - Payment processing: USD only
  - Shipping: US domestic initially
  - Compliance: US tax regulations, basic GDPR-ready architecture for future expansion

### Secondary Markets (Post-MVP)

1. **International SMBs:** Canada, UK, EU (6-12 months post-MVP)
2. **Direct-to-Consumer (D2C) Brands:** Larger brands seeking Shopify alternatives
3. **B2B Wholesale:** Businesses selling to other businesses with different pricing tiers

### Market Exclusions (MVP)

- Enterprise merchants (500+ employees)
- Complex B2B procurement workflows
- Multi-warehouse/multi-location inventory (initially)
- Dropshipping-specific features (may add later)

## 1.4 Competitive Positioning

### Competitive Landscape

| Platform | Strengths | Weaknesses | Vendo Differentiation |
|----------|-----------|------------|----------------------|
| **Shopify** | Market leader, extensive app ecosystem, ease of use | High transaction fees (2.9%+30¢ without Shopify Payments), limited customization, vendor lock-in | Lower transaction fees, open architecture, full data ownership, developer-friendly API-first approach |
| **WooCommerce** | WordPress integration, free core, highly customizable | Requires hosting management, plugin dependency, security burden on merchant | Managed SaaS model (no hosting hassle), built-in security, modern tech stack, integrated multi-tenancy |
| **BigCommerce** | No transaction fees, built-in features | Complex pricing tiers, slower innovation, less intuitive UX | Simpler pricing, faster feature velocity, modern Angular-based admin UX |
| **Square Online** | Integrated POS, simple setup | Limited customization, basic features only | More advanced features (variants, inventory management), better extensibility |

### Unique Value Propositions

1. **Developer-First Architecture**
   - Clean API design with comprehensive documentation
   - Webhook-driven extensibility
   - Open integration patterns
   - Modern tech stack (Angular + .NET Core)

2. **Transparent Pricing**
   - No transaction fees beyond payment processor costs
   - Predictable tiered subscriptions
   - No hidden charges for bandwidth or storage (within reasonable limits)

3. **Data Ownership & Privacy**
   - Merchants own all customer and order data
   - Export capabilities for all data
   - GDPR-ready data management tools
   - No data mining for advertising purposes

4. **Security & Compliance Built-In**
   - PCI-compliant hosted checkout (no merchant PCI burden)
   - Automated security updates
   - Regular third-party security audits
   - SOC 2 compliance roadmap

### Target Market Positioning

**"Shopify Power, WooCommerce Flexibility, Without the Complexity"**

Vendo positions itself as the modern middle ground:
- **Easier than WooCommerce:** Fully managed, no hosting/security burden
- **More flexible than Shopify:** Open APIs, extensible architecture, fair pricing
- **More affordable than BigCommerce:** Transparent pricing, lower entry point
- **More developer-friendly than all:** API-first design, modern stack, comprehensive docs

## 1.5 Success Metrics

### MVP Success Criteria (6 Months)

| Metric | Target | Measurement Method |
|--------|--------|-------------------|
| Active Merchants | 20 | Platform admin dashboard |
| Average Store Setup Time | < 60 minutes | Analytics tracking |
| System Uptime | 99.5% | Monitoring tools (Application Insights) |
| Critical Bugs | < 5 per month | Issue tracking (GitHub Issues) |
| Payment Success Rate | > 95% | Payment service analytics |
| API Response Time (p95) | < 200ms | Application Performance Monitoring |
| User Satisfaction (NPS) | > 40 | Quarterly surveys |

### Business Metrics (12 Months)

| Metric | Target | Measurement Method |
|--------|--------|-------------------|
| Monthly Recurring Revenue | $10K | Billing system |
| Customer Acquisition Cost | < $200 | Marketing analytics |
| Churn Rate | < 10% monthly | Retention analytics |
| Average Revenue Per User | $50/month | Billing analytics |
| Gross Merchandise Volume | $500K | Order analytics |

---

# 2. Stakeholder Analysis & Personas

## 2.1 Stakeholder Map

### Internal Stakeholders

| Stakeholder | Role | Primary Interest | Influence Level |
|-------------|------|------------------|-----------------|
| Product Owner | Strategic direction | Market fit, revenue growth | High |
| Lead Developer (Solo) | Implementation | Technical feasibility, code quality, maintainability | High |
| DevOps Engineer (Future) | Infrastructure | Scalability, reliability, deployment automation | Medium (future) |

### External Stakeholders

| Stakeholder | Role | Primary Interest | Influence Level |
|-------------|------|------------------|-----------------|
| Merchant (Store Owner) | Primary user | Ease of use, sales conversion, cost effectiveness | Critical |
| Merchant Staff | Secondary user | Daily operations efficiency | High |
| End Customers | Buyer | Purchase experience, security, convenience | High |
| Payment Providers | Integration partner | Transaction volume, compliance | Medium |
| Third-party Developers (Future) | Ecosystem builders | API clarity, extensibility | Medium (future) |

## 2.2 User Personas

### Persona 1: Sarah - The Artisan Entrepreneur

**Demographics:**
- Age: 32
- Location: Portland, Oregon
- Role: Founder & Owner
- Business: Handmade jewelry and accessories
- Revenue: $150K annually (currently via Etsy)
- Team: Solo (with part-time help during busy seasons)

**Background:**
Sarah started her jewelry business as a side hustle 5 years ago and went full-time 2 years ago. She currently sells primarily on Etsy but wants to build her own brand presence and reduce dependency on marketplace fees. She has basic computer skills but no technical background.

**Goals:**
- Launch a branded online store to complement Etsy presence
- Reduce marketplace fees (currently paying 6.5% to Etsy)
- Own customer relationships and build email list
- Provide better product photography and storytelling
- Manage inventory across both channels

**Pain Points:**
- Etsy fees eating into margins (loses $10K/year to fees)
- Limited branding control on marketplace
- Can't access customer email addresses for marketing
- Concerned about technical complexity of "real" e-commerce
- Limited budget for web development ($50-100/month max)

**Technical Proficiency:** Beginner
- Comfortable with: Social media, Etsy seller dashboard, email
- Uncomfortable with: Code, server management, complex software

**Key User Stories:**
- "I want to set up my store in one afternoon without hiring a developer"
- "I need to easily upload 50-100 products with multiple photos each"
- "I want my store to look professional and match my brand aesthetic"
- "I need to know immediately when someone makes a purchase"
- "I want to offer discount codes during holiday seasons"

**Success Criteria:**
- Can launch store in under 2 hours without help
- Spends less than $75/month on platform
- Reduces overall selling fees from 6.5% to under 3%
- Gets order notifications via email and SMS
- No payment processing hassles or security concerns

---

### Persona 2: Marcus - The Growth-Stage Retailer

**Demographics:**
- Age: 45
- Location: Austin, Texas
- Role: Co-owner & Operations Manager
- Business: Specialty coffee equipment and beans
- Revenue: $800K annually
- Team: 3 full-time (1 warehouse, 1 customer service, 1 marketing)

**Background:**
Marcus and his business partner have been selling coffee equipment through Amazon and their own WordPress/WooCommerce site for 4 years. Their WooCommerce site has become a maintenance headache - plugin conflicts, security concerns, and hosting issues. They're looking for a managed solution that's more professional than their current setup but more affordable and flexible than Shopify.

**Goals:**
- Migrate from WooCommerce to a managed platform
- Reduce time spent on technical maintenance (currently 5-10 hours/month)
- Improve site performance (current site is slow)
- Better inventory management across sales channels
- Add subscription products (coffee bean subscriptions)

**Pain Points:**
- WooCommerce plugin conflicts causing site crashes (2-3x per month)
- Security concerns after a failed hack attempt
- Slow page loads hurting conversion (current 4-5 seconds)
- Managing hosting, backups, and updates is time-consuming
- Need better reporting on sales trends and inventory

**Technical Proficiency:** Intermediate
- Comfortable with: WordPress admin, basic HTML/CSS editing, Google Analytics
- Uncomfortable with: Server administration, database management, complex code

**Key User Stories:**
- "I want to migrate my 200+ products without manually re-entering everything"
- "I need reliable uptime - downtime directly costs us sales"
- "I want detailed analytics on which products are selling and which aren't"
- "I need my marketing person to manage products without risking breaking the site"
- "I want to set up subscription products for recurring coffee deliveries"

**Success Criteria:**
- Zero unplanned downtime in first 6 months
- Page load times under 2 seconds
- Successful migration of all products and customer data
- Staff can manage daily operations without technical issues
- Clear ROI: saves 5+ hours/month in maintenance time

---

### Persona 3: Jennifer - The Multi-Channel Fashion Seller

**Demographics:**
- Age: 28
- Location: Los Angeles, California
- Role: Founder & Creative Director
- Business: Sustainable women's fashion brand
- Revenue: $300K annually (across Instagram Shop, Depop, and pop-up events)
- Team: 2 part-time (1 social media manager, 1 fulfillment assistant)

**Background:**
Jennifer launched her sustainable fashion line 3 years ago, building a strong Instagram following (50K followers). She currently sells through Instagram Shopping and Depop but wants a professional branded e-commerce site that she can direct her social media audience to. She's tech-savvy (millennial digital native) but not a developer.

**Goals:**
- Create a cohesive brand experience with custom design
- Integrate with Instagram and social media marketing
- Offer size and color variants for each clothing item
- Build customer loyalty program
- Prepare for scaling to $1M revenue in 2 years

**Pain Points:**
- Instagram Shopping is limited and doesn't reflect brand values
- Losing customers in handoff from Instagram to external checkout
- Can't offer bundles or promotional campaigns effectively
- No way to build customer database for email marketing
- Needs better insights into customer preferences and trends

**Technical Proficiency:** Intermediate-Advanced
- Comfortable with: Social media platforms, Instagram Creator Studio, Canva, basic analytics
- Comfortable learning: New software platforms, integrations, marketing tools
- Uncomfortable with: Backend coding, server management

**Key User Stories:**
- "I want my online store to have the same aesthetic as my Instagram feed"
- "I need to easily manage product variants (sizes XS-XL, 5 color options)"
- "I want to create bundle offers (buy 2 get 15% off)"
- "I need to integrate with my Instagram Shopping so inventory stays synced"
- "I want to reward loyal customers with a points program"

**Success Criteria:**
- Store launch within 3 weeks with custom branding
- Seamless Instagram integration for product tagging
- Conversion rate from Instagram to purchase > 2%
- Email list growth of 100+ subscribers per month
- Ability to run seasonal campaigns (holiday bundles, flash sales)

---

### Persona 4: David - The Platform Administrator

**Demographics:**
- Age: 35
- Location: Remote
- Role: Platform Operations & Support Lead (Future hire)
- Responsibility: Monitor platform health, support merchants, manage billing

**Background:**
David will be Vendo's first operations hire once the platform reaches 50+ active merchants. He has experience in SaaS customer success and basic technical troubleshooting but is not a developer. He needs powerful admin tools that don't require code-level access.

**Goals:**
- Monitor platform health and performance across all tenants
- Quickly troubleshoot merchant issues without developer involvement
- Manage billing, subscriptions, and payment reconciliation
- Identify and respond to fraud or abuse
- Generate reports for business stakeholders

**Pain Points:**
- Needs visibility into all tenants without compromising data isolation
- Must respond to merchant support requests quickly (< 2 hour response time)
- Requires ability to temporarily access merchant stores for troubleshooting
- Needs to identify failing payments or webhook issues proactively
- Must manage platform-level configurations (payment providers, email templates)

**Technical Proficiency:** Intermediate
- Comfortable with: Admin dashboards, SQL queries (basic), support ticketing systems
- Uncomfortable with: Production deployments, code changes, infrastructure management

**Key User Stories:**
- "I want a dashboard showing all active tenants and their health status"
- "I need to search across all stores to find a specific order for customer support"
- "I want automated alerts when webhooks fail or payments are stuck"
- "I need to safely impersonate a merchant to troubleshoot issues they're experiencing"
- "I want to generate monthly usage reports for billing reconciliation"

**Success Criteria:**
- Can resolve 80% of merchant issues without developer escalation
- Average response time < 2 hours during business hours
- No accidental data leakage between tenants
- Clear audit trail of all administrative actions
- Automated daily reports on platform health metrics

---

### Persona 5: Alex - The Third-Party Developer (Future)

**Demographics:**
- Age: 29
- Location: Toronto, Canada
- Role: Independent Software Developer
- Business: Builds integrations and plugins for e-commerce platforms
- Experience: 5 years building Shopify apps and WooCommerce plugins

**Background:**
Alex is an experienced e-commerce ecosystem developer who builds integrations, analytics tools, and automation plugins. Once Vendo reaches market traction (100+ merchants), Alex represents the type of third-party developer who will build the extension ecosystem. His success is critical to Vendo's long-term competitiveness.

**Goals:**
- Build profitable apps/integrations for Vendo merchants
- Access comprehensive, well-documented APIs
- Publish apps in future Vendo marketplace
- Get paid fairly for valuable integrations
- Build reputation in emerging platform ecosystem

**Pain Points:**
- Needs clear, accurate API documentation
- Requires sandbox/test environments for development
- Must understand authentication and authorization patterns
- Needs webhook reliability for real-time integrations
- Wants transparent marketplace policies and revenue sharing

**Technical Proficiency:** Expert
- Comfortable with: REST APIs, OAuth 2.0, webhooks, modern web frameworks
- Prefers: Clear API contracts, versioning, comprehensive error handling
- Expects: Postman collections, code samples, active developer community

**Key User Stories:**
- "I want complete API documentation with interactive examples"
- "I need a free sandbox tenant to develop and test my integration"
- "I want to register webhooks for order and product events"
- "I need to authenticate on behalf of merchants using OAuth 2.0"
- "I want to publish my app in the marketplace and handle billing through Vendo"

**Success Criteria:**
- Can build a working integration in < 2 days
- API reliability > 99.9% uptime
- Webhook delivery within 5 seconds of events
- No breaking API changes without 6-month deprecation notice
- Marketplace app approval within 7 days

---

## 2.3 Stakeholder Communication Plan

| Stakeholder Group | Communication Method | Frequency | Owner |
|------------------|---------------------|-----------|-------|
| Merchants (Active) | Email newsletter, in-app notifications | Weekly | Product Owner |
| Merchants (Support) | Support portal, email, chat (future) | On-demand | Platform Admin |
| Pilot Merchants | Direct email, monthly calls | Monthly | Product Owner |
| Developers (Future) | Developer blog, API changelog, forum | Bi-weekly | Tech Lead |
| Internal Team | Slack, weekly standups | Daily/Weekly | Product Owner |

---

# 3. Functional Requirements by Domain

## 3.1 Requirements Framework

### Requirement Format

Each functional requirement follows this structure:

**FR-[DOMAIN]-[NUMBER]:** Requirement Title

**Priority:** Must Have / Should Have / Could Have / Won't Have (MoSCoW)

**Description:** Detailed requirement description

**User Story:**
```
Given [context/precondition]
When [action/event]
Then [expected outcome]
```

**Acceptance Criteria:**
- Specific, testable criteria
- Edge cases and error conditions
- Performance expectations where applicable

**Dependencies:** Other requirements or external systems

**Technical Notes:** Implementation considerations (for development team)

---

## 3.2 Domain: Tenant Management

### Overview
Tenant Management encompasses merchant onboarding, store configuration, subscription management, and multi-tenant data isolation. This is the foundational domain that enables the platform's multi-tenancy model.

---

### FR-TNT-001: Merchant Self-Service Registration

**Priority:** Must Have (MVP)

**Description:**
Merchants must be able to register for the platform and create their first store through a self-service onboarding flow without requiring manual approval or intervention.

**User Story:**
```
Given I am a new merchant visiting the Vendo signup page
When I provide my email, password, store name, and subdomain
Then my account is created, my store is provisioned, and I receive a verification email
And I can log into the admin dashboard immediately
```

**Acceptance Criteria:**
- Registration form validates email format, password strength (8+ chars, mixed case, number, special char)
- Store subdomain is validated for uniqueness and format (alphanumeric, hyphens only, 3-63 chars)
- Store subdomain cannot be changed after creation (MVP - may add later)
- Email verification link is sent within 30 seconds
- New tenant record created with status "Active" and default settings
- Default merchant admin user created and linked to tenant
- Tenant receives default store settings: USD currency, English language, US timezone
- System generates unique TenantId (GUID)
- Registration completes in < 5 seconds (excluding email send)

**Business Rules:**
- One merchant email can only register one store initially (MVP limitation)
- Store name and subdomain must be unique across platform
- Default subscription tier: Free (with 14-day trial of Pro features)
- Subdomain format: {store-name}.vendo.app (configurable base domain)

**Dependencies:**
- FR-IDN-001: Identity Server user authentication
- FR-TNT-005: Default store settings initialization

**Technical Notes:**
- Implement email verification using ASP.NET Core Identity
- Store TenantId in all user claims for downstream authorization
- Create default categories and sample product for new stores
- Log tenant creation event for analytics

---

### FR-TNT-002: Store Settings Management

**Priority:** Must Have (MVP)

**Description:**
Merchants must be able to configure essential store settings including store name, contact information, currency, timezone, and tax settings.

**User Story:**
```
Given I am logged in as a merchant admin
When I navigate to Store Settings and update my store information
Then my changes are saved and reflected on my storefront immediately
And my customers see the updated information on checkout
```

**Acceptance Criteria:**
- Merchant can update: store name, contact email, phone, address, currency, timezone, default tax rate
- Store name changes do not affect subdomain (subdomain is permanent)
- Currency cannot be changed after first order is placed (prevent data inconsistencies)
- Timezone affects order timestamps and analytics displays
- Tax settings support: no tax, single tax rate (%), or tax-exempt
- Changes are validated before saving
- Settings page loads in < 1 second
- Changes take effect immediately (no caching delay)

**Business Rules:**
- Currency lock: Once first paid order exists, currency cannot be changed
- Tax rate: 0-100%, supports up to 2 decimal places (e.g., 8.25%)
- Contact email used for order notifications and customer service replies

**Dependencies:**
- FR-TNT-001: Store must exist
- FR-ORD-003: Order calculations use tax settings

**Technical Notes:**
- Store settings in StoreSettings table with TenantId foreign key
- Cache store settings in Redis for performance (invalidate on update)
- Validate currency against ISO 4217 codes
- Validate timezone against IANA timezone database

---

### FR-TNT-003: Store Branding and Theming

**Priority:** Should Have (MVP)

**Description:**
Merchants must be able to customize their store's visual branding including logo, colors, and basic theme settings to match their brand identity.

**User Story:**
```
Given I am a merchant admin
When I upload my logo and select brand colors
Then my storefront displays my branding consistently
And my customers recognize my brand throughout the shopping experience
```

**Acceptance Criteria:**
- Merchant can upload logo image (PNG, JPG, SVG formats, max 2MB)
- Merchant can select primary color (hex color picker)
- Merchant can select accent color for buttons/CTAs
- Logo displayed in storefront header and checkout pages
- Colors applied consistently across all storefront pages
- Logo is automatically resized and optimized for web (lazy loading)
- Preview changes before publishing
- Branding changes take effect immediately after save

**Business Rules:**
- Free tier: Limited to 3 color changes per month
- Pro tier: Unlimited color changes
- Logo file size max 2MB, auto-compressed if larger
- Supported formats: PNG, JPG, JPEG, SVG
- Image CDN caching with cache invalidation on upload

**Dependencies:**
- FR-TNT-002: Store settings infrastructure
- Cloud storage integration for image hosting (Azure Blob / AWS S3)

**Technical Notes:**
- Store logo URL in database, actual file in blob storage
- Generate multiple logo sizes (thumbnail, standard, retina)
- Implement color contrast validation for accessibility (WCAG AA)
- Use CSS custom properties for theme color application

---

### FR-TNT-004: Subscription Tier Management

**Priority:** Must Have (MVP)

**Description:**
The platform must enforce subscription tier limits and features, allowing merchants to upgrade/downgrade their plans and managing access to tier-specific functionality.

**User Story:**
```
Given I am a merchant on the Free tier
When I try to access a Pro feature (e.g., advanced analytics)
Then I am prompted to upgrade my subscription
And I can upgrade with one click through the billing page
```

**Acceptance Criteria:**
- Three tiers implemented: Free, Starter ($29/mo), Pro ($79/mo)
- Free tier limits: 50 products, 100 orders/month, basic analytics, 1 admin user
- Starter tier limits: 500 products, 1000 orders/month, advanced analytics, 3 admin users
- Pro tier limits: Unlimited products, unlimited orders, all features, 10 admin users
- System enforces limits in real-time (e.g., blocks product creation at limit)
- Merchants can view current usage vs. limits in dashboard
- Upgrade flow is self-service with immediate access after payment
- Downgrade scheduled for next billing cycle (not immediate)

**Business Rules:**
- New signups get 14-day trial of Pro tier, then downgrade to Free if no payment
- Exceeding tier limits blocks functionality (cannot add products, process orders)
- Tier changes log audit events
- Usage resets monthly (orders/month counter)

**Dependencies:**
- FR-PAY-005: Subscription billing integration
- FR-TNT-001: Tenant creation with default tier

**Technical Notes:**
- Store subscription tier in Tenant record
- Implement tier checking middleware for feature access
- Use feature flags for tier-specific functionality
- Create background job to reset monthly counters

---

### FR-TNT-005: Multi-Tenant Data Isolation

**Priority:** Must Have (MVP) - Critical Security Requirement

**Description:**
The platform must ensure complete data isolation between tenants, preventing any merchant from accessing or modifying another merchant's data through the application, API, or database queries.

**User Story:**
```
Given I am logged in as Merchant A
When I make any API request to view or modify data
Then I can only access data belonging to Tenant A
And I receive 403 Forbidden if I attempt to access another tenant's data
```

**Acceptance Criteria:**
- All database queries automatically filter by TenantId
- API requests validate TenantId from JWT token matches requested resource
- Attempting to access cross-tenant data returns 403 Forbidden error
- Admin panel shows only current tenant's data
- File uploads isolated to tenant-specific storage containers/folders
- Audit logs include TenantId for all operations
- Database constraints prevent accidental cross-tenant writes
- Integration tests verify tenant isolation

**Business Rules:**
- Every entity must include TenantId foreign key
- TenantId sourced from authenticated user's JWT claims
- Platform admins can access all tenants but actions are logged
- Soft deletes maintain TenantId for audit trail

**Dependencies:**
- FR-IDN-001: Authentication provides TenantId in token claims
- 02_BUSINESS_RULES.md: Tenant isolation rules

**Technical Notes:**
- Implement ITenantContext service to provide current TenantId
- EF Core global query filters on all DbSets
- Middleware to inject TenantId into request context
- Unit tests verify query filters are applied
- Consider row-level security (RLS) in SQL Server for defense in depth

---

### FR-TNT-006: Store Domain Management

**Priority:** Could Have (Post-MVP)

**Description:**
Merchants should be able to configure a custom domain (e.g., shop.mybrand.com) instead of using the default subdomain (mybrand.vendo.app).

**User Story:**
```
Given I am a Pro tier merchant
When I configure my custom domain and update my DNS settings
Then my storefront is accessible at my custom domain
And SSL certificate is automatically provisioned
```

**Acceptance Criteria:**
- Merchant can add custom domain in Store Settings
- System validates domain ownership via DNS TXT record
- SSL certificate automatically provisioned via Let's Encrypt
- Both custom domain and subdomain remain accessible
- Primary domain setting determines canonical URLs
- Domain changes propagate within 5 minutes

**Business Rules:**
- Custom domains available only to Pro tier
- Maximum 1 custom domain per store (MVP)
- Subdomain cannot be deleted (always available as fallback)
- SSL certificate renewal automated

**Dependencies:**
- FR-TNT-004: Pro tier subscription
- Infrastructure: Load balancer, DNS management, Let's Encrypt integration

**Technical Notes:**
- Store custom domain in StoreSettings table
- Implement DNS verification workflow
- Use cert-manager or similar for certificate automation
- Update CORS policies to allow custom domains

---

## 3.3 Domain: Catalog Management

### Overview
Catalog Management covers all product-related functionality including product creation, variants, categories, inventory management, and product publishing workflow.

---

### FR-CAT-001: Product Creation and Basic Management

**Priority:** Must Have (MVP)

**Description:**
Merchants must be able to create, edit, and delete products with essential attributes including name, description, price, SKU, and images.

**User Story:**
```
Given I am a merchant admin
When I create a new product with name, description, price, and images
Then the product is saved to my catalog
And I can edit or delete it later
And the product is not visible to customers until I publish it
```

**Acceptance Criteria:**
- Product form includes: name (required, max 200 chars), description (rich text, max 5000 chars), SKU (unique per tenant, max 50 chars), base price (required, decimal, positive)
- Support multiple product images (min 1, max 10 per product)
- Images can be uploaded (PNG, JPG, WEBP, max 5MB each)
- Image upload includes drag-and-drop reordering
- Product status: Draft, Published, Archived
- Only Published products visible on storefront
- SKU automatically generated if not provided (format: PROD-{timestamp})
- Rich text editor supports: bold, italic, lists, links (no script tags for security)
- Product save completes in < 2 seconds

**Business Rules:**
- SKU must be unique within tenant (case-insensitive)
- Price must be positive, supports 2 decimal places
- Product belongs to exactly one tenant
- Deleting product is soft delete (set IsActive = false)
- Cannot delete product referenced in existing orders (return validation error)
- Published date tracks when product first published

**Dependencies:**
- FR-TNT-001: Merchant must have active tenant
- FR-CAT-002: Category assignment
- Cloud storage for images

**Technical Notes:**
- Store Product entity with TenantId foreign key
- Implement ProductImage child entity (one-to-many)
- Use Azure Blob Storage or AWS S3 for images
- Generate image thumbnails automatically (150px, 400px, 800px)
- Sanitize rich text description to prevent XSS
- Index SKU for fast lookups

---

### FR-CAT-002: Product Categories and Organization

**Priority:** Must Have (MVP)

**Description:**
Merchants must be able to create categories and assign products to multiple categories for storefront organization and navigation.

**User Story:**
```
Given I am a merchant admin
When I create categories like "Women's Clothing" and "Summer Collection"
Then I can assign products to one or more categories
And customers can browse products by category on my storefront
```

**Acceptance Criteria:**
- Merchant can create, edit, delete categories
- Category attributes: name (required, max 100 chars), description (optional, max 500 chars), display order (integer)
- Products can belong to multiple categories (many-to-many relationship)
- Categories displayed in storefront navigation
- Category page shows all products in that category
- Support up to 50 categories per store (MVP limit)
- Category names must be unique within tenant

**Business Rules:**
- Cannot delete category with assigned products (must reassign first)
- Category belongs to exactly one tenant
- Categories sorted by display order (ascending), then alphabetically
- Uncategorized products accessible via "All Products" default category

**Dependencies:**
- FR-CAT-001: Products must exist to assign to categories

**Technical Notes:**
- Category entity with TenantId foreign key
- ProductCategory junction table for many-to-many
- Implement soft delete for categories
- Cache category tree for performance

---

### FR-CAT-003: Product Variants (Size, Color, etc.)

**Priority:** Must Have (MVP)

**Description:**
Merchants must be able to create product variants (e.g., different sizes, colors) with unique SKUs, prices, and inventory levels for each variant.

**User Story:**
```
Given I am selling a t-shirt in multiple sizes and colors
When I create a product with variants for Size (S, M, L) and Color (Red, Blue, Black)
Then the system generates all combinations (9 variants total)
And I can set individual prices and inventory for each variant
And customers can select their size and color at checkout
```

**Acceptance Criteria:**
- Support up to 3 variant types per product (e.g., Size, Color, Material)
- Each variant type has multiple options (e.g., Size: S, M, L, XL)
- System auto-generates all variant combinations
- Each variant has: unique SKU, price (can inherit from base product), inventory count, barcode (optional)
- Variants can be individually enabled/disabled
- Out-of-stock variants show as unavailable on storefront
- Variant selection interface on storefront (dropdown or button picker)
- Max 100 total variants per product (e.g., 3 sizes × 5 colors × 7 materials = 105 would exceed limit)

**Business Rules:**
- Variant SKU must be unique within tenant
- Variant price defaults to base product price but can be overridden
- Inventory tracked per variant (not base product)
- At least one variant must be enabled if product is published
- Deleting variant type removes all associated variants (with confirmation)

**Dependencies:**
- FR-CAT-001: Base product must exist
- FR-CAT-005: Inventory management

**Technical Notes:**
- ProductVariant entity with ProductId foreign key
- VariantOption entity for option values (e.g., "Large", "Red")
- Cartesian product algorithm to generate variant combinations
- Display variants as matrix grid in admin for easy bulk editing
- Use JSON column for variant attributes for flexibility

---

### FR-CAT-004: Product Image Management

**Priority:** Should Have (MVP)

**Description:**
Merchants must be able to upload, manage, and reorder product images with automatic optimization and responsive delivery.

**User Story:**
```
Given I am creating a product
When I upload 5 product images
Then the images are optimized, resized, and stored securely
And I can drag-and-drop to reorder them
And the first image becomes the primary product image
```

**Acceptance Criteria:**
- Support drag-and-drop image upload
- Multiple image upload (max 10 per product)
- Supported formats: JPG, PNG, WEBP (convert other formats to WEBP)
- Max file size: 5MB per image
- Automatic image optimization: compress, resize, generate responsive sizes
- Generated sizes: thumbnail (150x150), medium (400x400), large (800x800), original
- Drag-and-drop reordering
- Delete individual images
- Set primary image (first image by default)
- Image URLs served from CDN
- Alt text field for accessibility (max 200 chars)

**Business Rules:**
- Free tier: Max 5 images per product, 100 total images per store
- Starter tier: Max 10 images per product, 500 total images per store
- Pro tier: Max 10 images per product, unlimited total images
- Images stored with unique filenames to prevent collisions
- Deleted images removed from storage after 30-day grace period

**Dependencies:**
- FR-CAT-001: Product must exist
- Cloud storage (Azure Blob / AWS S3)
- CDN configuration (Azure CDN / CloudFront)

**Technical Notes:**
- Use ImageSharp library for server-side image processing
- Store original and generated sizes in blob storage
- ProductImage table stores URLs and metadata
- Implement background job for image processing (async upload)
- Generate WebP format for modern browsers, fallback to JPG
- Implement lazy loading on storefront

---

### FR-CAT-005: Inventory Management

**Priority:** Must Have (MVP)

**Description:**
Merchants must be able to track and manage inventory levels for products and variants, with automatic deduction on order placement and prevention of overselling.

**User Story:**
```
Given I have a product with 10 units in stock
When a customer places an order for 2 units
Then inventory is reduced to 8 units automatically
And I receive a low stock alert when inventory reaches my threshold
And customers cannot purchase when inventory reaches 0
```

**Acceptance Criteria:**
- Track inventory quantity per product (simple) or per variant (variant products)
- Inventory fields: available quantity (integer), low stock threshold (integer, default 5), track inventory toggle (boolean)
- Inventory deducted automatically when order status changes to "Paid"
- Prevent negative inventory (validation error if quantity unavailable)
- "Out of Stock" label displayed on storefront when quantity = 0
- Low stock alert badge in admin when quantity <= threshold
- Option to allow/disallow backorders (MVP: disallow)
- Inventory history log (track additions, deductions, adjustments)

**Business Rules:**
- Inventory cannot go negative
- Inventory deduction is transactional (rolled back if order payment fails)
- Cancelled or refunded orders restore inventory
- Manual inventory adjustments require reason/note
- Inventory changes log audit trail with timestamp and user

**Dependencies:**
- FR-CAT-001: Product must exist
- FR-CAT-003: Variants (if applicable)
- FR-ORD-002: Order placement triggers inventory deduction

**Technical Notes:**
- Store inventory in ProductInventory table (separate from Product for locking)
- Use database transactions for inventory updates
- Implement optimistic concurrency control to prevent race conditions
- Background job to send low stock notifications
- Consider implementing reserved inventory for in-progress checkouts (future)

---

### FR-CAT-006: Product Search and Filtering

**Priority:** Should Have (MVP)

**Description:**
Merchants must be able to search and filter products in the admin panel, and customers must be able to search and filter products on the storefront.

**User Story:**
```
Given I am a customer on a storefront
When I search for "red dress"
Then I see all products matching "red dress" in name or description
And I can filter results by category, price range, and availability
```

**Acceptance Criteria:**
- Admin search: Search by product name, SKU, description (partial match, case-insensitive)
- Admin filters: Category, status (Draft/Published/Archived), inventory status (In Stock/Low Stock/Out of Stock)
- Storefront search: Search by product name, description
- Storefront filters: Category, price range (min-max), availability (in stock only toggle)
- Search results display within 1 second for up to 1000 products
- Pagination: 20 products per page (configurable)
- Sort options: Relevance (default), Price (low to high), Price (high to low), Name (A-Z), Newest first

**Business Rules:**
- Search indexes only Published products on storefront
- Search is tenant-scoped (cannot search across tenants)
- Special characters in search sanitized to prevent injection

**Dependencies:**
- FR-CAT-001: Products
- FR-CAT-002: Categories

**Technical Notes:**
- Implement full-text search using SQL Server Full-Text Index
- Consider Elasticsearch for advanced search (post-MVP)
- Cache frequently searched terms
- Implement search analytics to track popular queries

---

### FR-CAT-007: Bulk Product Import/Export

**Priority:** Could Have (Post-MVP)

**Description:**
Merchants should be able to import products from CSV files and export their catalog for backup or migration purposes.

**User Story:**
```
Given I have 200 products in a spreadsheet
When I upload a CSV file with product data
Then all products are imported into my catalog
And I receive a report of successful and failed imports
```

**Acceptance Criteria:**
- Export: Download all products as CSV (includes name, SKU, price, inventory, category)
- Import: Upload CSV to create or update products
- CSV template provided for download
- Import validation: SKU uniqueness, required fields, data types
- Import preview before final commit
- Import runs asynchronously with progress indicator
- Import summary: total processed, successful, failed (with error details)
- Max 1000 products per import (MVP)

**Business Rules:**
- Import matches existing products by SKU (update) or creates new
- Cannot import products exceeding tier limits
- Import overwrites existing data (with confirmation)

**Dependencies:**
- FR-CAT-001: Products
- FR-TNT-004: Tier limits

**Technical Notes:**
- Use CsvHelper library for parsing
- Implement background job for large imports
- Store import history and error logs
- Validate CSV structure before processing

---

## 3.4 Domain: Order Management

### Overview
Order Management covers the customer order lifecycle from cart to fulfillment, including order creation, status tracking, and merchant order processing.

---

### FR-ORD-001: Shopping Cart Functionality

**Priority:** Must Have (MVP)

**Description:**
Customers must be able to add products to a shopping cart, modify quantities, remove items, and view cart totals before proceeding to checkout.

**User Story:**
```
Given I am a customer browsing a storefront
When I add products to my cart and adjust quantities
Then I see my cart total updated in real-time
And I can proceed to checkout when ready
```

**Acceptance Criteria:**
- Add product (with variant selection if applicable) to cart
- Update quantity of cart items (min 1, max 99 per item)
- Remove items from cart
- Cart displays: product name, variant options, quantity, unit price, line total, cart subtotal
- Cart persists for 7 days for guest users (cookie-based)
- Cart persists indefinitely for logged-in customers
- Cart icon shows item count badge
- Cart quantity updates reflect inventory availability (cannot exceed stock)
- Mini cart preview on hover or click

**Business Rules:**
- Cart max 50 line items (MVP limit)
- Cart validates inventory availability before checkout
- Prices in cart reflect current product prices (may change since cart was created)
- Out-of-stock items flagged in cart with option to remove

**Dependencies:**
- FR-CAT-001: Products must exist
- FR-CAT-005: Inventory validation

**Technical Notes:**
- Store cart in browser localStorage for guests
- Store cart in database for logged-in users (CartItem table)
- Implement cart abandonment analytics (post-MVP)
- Use TenantId scoping for cart data

---

### FR-ORD-002: Checkout Process

**Priority:** Must Have (MVP)

**Description:**
Customers must complete a secure checkout process by providing shipping information, selecting payment method, reviewing order, and confirming purchase.

**User Story:**
```
Given I have items in my cart
When I proceed through checkout with my shipping address
Then I am redirected to a secure payment page
And after successful payment, I receive an order confirmation
```

**Acceptance Criteria:**
- Checkout steps: 1) Shipping Info, 2) Review & Payment
- Shipping info fields: email, first name, last name, address line 1, address line 2 (optional), city, state/province, postal code, country, phone
- Email validation and required field checks
- Order summary displays: items, quantities, unit prices, subtotal, tax, shipping (if applicable), total
- Tax calculated based on store tax settings (FR-TNT-002)
- "Place Order" button redirects to hosted payment checkout (Stripe Checkout)
- Order created with status "Pending" before payment redirect
- After successful payment (webhook received), order status updated to "Paid"
- Customer receives order confirmation email
- Guest checkout supported (no account creation required)

**Business Rules:**
- Shipping cost: $0 (free shipping for MVP)
- Tax applied based on store settings
- Order total calculated server-side (never trust client)
- Order cannot be placed if any item exceeds available inventory
- Order number format: ORD-{TenantId-short}-{timestamp}-{random}

**Dependencies:**
- FR-ORD-001: Cart must exist
- FR-PAY-001: Payment provider integration
- FR-TNT-002: Tax settings

**Technical Notes:**
- Create Order entity with status "Pending" before payment
- Store order items as OrderItem records (snapshot prices at time of purchase)
- Implement order total calculation service with unit tests
- Validate inventory before creating order (with row locking)
- Send order confirmation email asynchronously via background job

---

### FR-ORD-003: Order Status and Fulfillment Workflow

**Priority:** Must Have (MVP)

**Description:**
Merchants must be able to view orders, update order status, and mark orders as fulfilled with shipping tracking information.

**User Story:**
```
Given I am a merchant admin
When I view my orders dashboard
Then I see all orders with their current status
And I can mark orders as shipped with tracking numbers
```

**Acceptance Criteria:**
- Order status values: Pending, Paid, Shipped, Delivered, Cancelled
- Order status transitions (enforced):
  - Pending → Paid (automatic via webhook)
  - Paid → Shipped (manual by merchant)
  - Shipped → Delivered (manual by merchant or future carrier integration)
  - Pending/Paid → Cancelled (manual by merchant)
- Orders list view shows: order number, customer name, date, status, total
- Orders filterable by: status, date range, customer name/email
- Order detail view shows: customer info, shipping address, order items, totals, status history, payment info
- Mark as Shipped form: carrier (dropdown: USPS, UPS, FedEx, Other), tracking number, shipping date
- Shipped orders trigger customer notification email with tracking info
- Status change logs timestamp and user who made change

**Business Rules:**
- Cannot change status to invalid state (e.g., Shipped → Pending not allowed)
- Cancelled orders restore inventory (FR-CAT-005)
- Paid orders are immutable (line items cannot be modified)
- Status changes create audit log entries

**Dependencies:**
- FR-ORD-002: Orders must exist
- FR-CAT-005: Inventory restoration on cancellation

**Technical Notes:**
- Store order status in Order.Status enum column
- OrderStatusHistory table tracks status changes with timestamp, user, notes
- Implement state machine pattern for status transitions
- Background job sends customer notifications on status change
- Index orders by TenantId and Status for fast queries

---

### FR-ORD-004: Order Search and Management

**Priority:** Should Have (MVP)

**Description:**
Merchants must be able to search, filter, and export orders for reporting and fulfillment purposes.

**User Story:**
```
Given I am a merchant admin
When I search for orders by customer email or order number
Then I see matching orders instantly
And I can export filtered orders to CSV
```

**Acceptance Criteria:**
- Search by: order number, customer name, customer email (partial match)
- Filter by: status, date range (from/to), payment status, total amount range
- Sort by: date (newest first default), total (high to low / low to high), status
- Pagination: 50 orders per page
- Bulk actions: Export to CSV, bulk status update (future)
- Export includes: order number, date, customer info, items, totals, status
- Search results load within 1 second for up to 10,000 orders

**Business Rules:**
- Orders scoped to current tenant only
- Exported data excludes payment card details (only last 4 digits if stored)

**Dependencies:**
- FR-ORD-003: Orders and status management

**Technical Notes:**
- Implement full-text search on customer name and email
- Index order number for fast exact match
- Use date range indexes for performance
- Background job for CSV export (async download link via email for large exports)

---

### FR-ORD-005: Order Refund and Cancellation

**Priority:** Could Have (Post-MVP)

**Description:**
Merchants should be able to cancel orders and process full or partial refunds through the payment provider.

**User Story:**
```
Given I am a merchant admin viewing a paid order
When I issue a refund for the full amount
Then the payment provider processes the refund
And the order status updates to Refunded
And inventory is restored
```

**Acceptance Criteria:**
- Cancel order option for Pending or Paid orders
- Refund options: Full refund, Partial refund (specify amount)
- Refund initiates payment provider API call
- Refund status tracked: Pending, Successful, Failed
- Successful refund updates order status to Refunded
- Inventory restored for refunded/cancelled items
- Customer receives refund confirmation email
- Refund reason field required (free text, max 500 chars)

**Business Rules:**
- Cannot refund Cancelled or already Refunded orders
- Cannot refund Shipped orders without admin override (assume return process)
- Partial refunds cannot exceed order total
- Refunds create audit log with reason and user

**Dependencies:**
- FR-ORD-003: Order must exist
- FR-PAY-002: Payment provider refund API
- FR-CAT-005: Inventory restoration

**Technical Notes:**
- OrderRefund entity tracks refund attempts
- Implement idempotent refund processing (prevent duplicate refunds)
- Webhook from payment provider confirms refund success
- Handle refund failures gracefully with retry mechanism

---

## 3.5 Domain: Payment Processing

### Overview
Payment Processing covers integration with hosted payment providers using tokenized/hosted checkout to ensure PCI compliance without storing card data.

---

### FR-PAY-001: Hosted Checkout Integration (Stripe)

**Priority:** Must Have (MVP)

**Description:**
The platform must integrate with Stripe Checkout to enable secure, PCI-compliant payment processing without handling raw card data.

**User Story:**
```
Given I am a customer completing checkout
When I click "Place Order"
Then I am redirected to Stripe's hosted checkout page
And after successful payment, I am redirected back with order confirmation
```

**Acceptance Criteria:**
- Integration with Stripe Checkout (hosted payment page)
- Create Stripe Checkout Session with order details (items, amounts, metadata)
- Redirect customer to Stripe checkout URL
- Stripe redirects back to success URL after payment
- Stripe sends webhook on payment success/failure
- Webhook validates Stripe signature for security
- Payment record created with: OrderId, ProviderName (Stripe), ProviderTransactionId, Amount, Currency, Status
- Order status updated to "Paid" on successful webhook
- Failed payments keep order in "Pending" with error message

**Business Rules:**
- Only USD currency supported (MVP)
- Payment amount must match order total (server-side validation)
- Webhook processing is idempotent (deduplicate by ProviderTransactionId)
- Payment provider transaction fee passed to customer or absorbed by merchant (configurable per store)

**Dependencies:**
- FR-ORD-002: Order must be created before payment
- FR-TNT-002: Store settings for currency
- Stripe account and API keys (stored in secure configuration)

**Technical Notes:**
- Use Stripe .NET SDK for API integration
- Store Stripe webhook secret in environment variables
- Implement webhook signature verification per Stripe docs
- PaymentTransaction table stores payment attempts
- Set Stripe metadata with OrderId and TenantId for reconciliation
- Implement retry logic for failed webhooks (Stripe retries automatically)
- Log all webhook payloads (sanitize sensitive data)

---

### FR-PAY-002: Payment Status Tracking

**Priority:** Must Have (MVP)

**Description:**
The platform must track payment status throughout the transaction lifecycle and handle edge cases like abandoned payments, failures, and timeouts.

**User Story:**
```
Given I am a merchant admin
When I view an order's payment details
Then I see the payment status, transaction ID, and any error messages
And I can manually reconcile payments if needed
```

**Acceptance Criteria:**
- Payment status values: Pending, Success, Failed, Refunded
- Payment detail view shows: status, provider name, transaction ID, amount, currency, timestamp, error message (if failed)
- Abandoned payments (no webhook received within 1 hour) flagged for manual review
- Payment retry option for failed payments (generates new checkout session)
- Payment reconciliation report for accounting purposes

**Business Rules:**
- One successful payment per order (prevent double charging)
- Failed payments do not block new payment attempts
- Pending payments timeout after 24 hours (order cancelled automatically)

**Dependencies:**
- FR-PAY-001: Payment integration
- FR-ORD-003: Order status linked to payment status

**Technical Notes:**
- PaymentTransaction.Status enum
- Background job to check abandoned payments (no webhook after 1 hour)
- Payment audit log for compliance
- Admin dashboard widget showing payment success rate

---

### FR-PAY-003: Multi-Provider Support (Future)

**Priority:** Won't Have (MVP) - Post-MVP Feature

**Description:**
The platform should support multiple payment providers (PayPal, Square, Authorize.net) allowing merchants to choose their preferred provider.

**User Story:**
```
Given I am a merchant admin
When I configure my store settings
Then I can select Stripe, PayPal, or Square as my payment provider
And customers see my selected payment method at checkout
```

**Acceptance Criteria:**
- Provider selection in Store Settings
- Each provider has its own configuration (API keys, webhook URLs)
- Checkout redirects to selected provider's hosted page
- Unified webhook handling for all providers
- Payment reconciliation across providers

**Business Rules:**
- Only one active provider per store (MVP limitation)
- Switching providers requires completing pending payments on old provider
- Provider configuration encrypted in database

**Dependencies:**
- FR-PAY-001: Core payment infrastructure
- Provider-specific SDKs and accounts

**Technical Notes:**
- Implement IPaymentProvider abstraction
- Factory pattern to instantiate correct provider
- Provider-specific webhook endpoints
- Normalize provider responses to common PaymentTransaction model

---

### FR-PAY-004: Payment Webhook Security and Reliability

**Priority:** Must Have (MVP) - Critical Security Requirement

**Description:**
Payment webhooks must be secured against spoofing, verified for authenticity, and processed reliably with idempotency guarantees.

**User Story:**
```
Given a payment webhook is received from Stripe
When the system validates the webhook signature
Then only authentic webhooks are processed
And duplicate webhooks are ignored
```

**Acceptance Criteria:**
- Webhook signature verification using provider's secret key
- Reject webhooks with invalid signatures (return 401)
- Deduplicate webhooks by provider event ID
- Log all webhook attempts (successful and failed)
- Retry failed webhook processing (transient errors)
- Alert on repeated webhook failures
- Webhook processing completes within 5 seconds

**Business Rules:**
- Webhooks from unknown IPs logged but not automatically rejected (providers use dynamic IPs)
- Webhook payload stored for 90 days for audit
- Failed webhook processing does not block provider retries

**Dependencies:**
- FR-PAY-001: Payment integration
- 07_SECURITY_AND_COMPLIANCE.md: Webhook security requirements

**Technical Notes:**
- Use Stripe signature verification library
- Store processed event IDs in WebhookEvent table (unique constraint)
- Implement exponential backoff for retry logic
- Monitor webhook processing latency and failure rate
- Consider webhook queue for high-volume scenarios (post-MVP)

---

### FR-PAY-005: Subscription Billing (Platform Subscriptions)

**Priority:** Must Have (MVP)

**Description:**
The platform must charge merchants for their subscription tier (Free, Starter, Pro) on a monthly recurring basis.

**User Story:**
```
Given I am a merchant on the Starter plan ($29/mo)
When my monthly billing date arrives
Then my payment method is charged automatically
And my subscription remains active
```

**Acceptance Criteria:**
- Integrate with Stripe Billing for subscription management
- Subscription tiers: Free ($0), Starter ($29/mo), Pro ($79/mo)
- Merchants add payment method during upgrade
- Automatic monthly billing on subscription anniversary
- Failed payment triggers: retry (3 attempts), notification email, grace period (7 days), downgrade to Free
- Merchants can view billing history and invoices
- Merchants can update payment method
- Merchants can cancel subscription (downgrade to Free)

**Business Rules:**
- Free tier does not require payment method
- 14-day trial of Pro tier for new signups (no charge)
- Trial converts to Free if no payment method added
- Cancellation takes effect at end of current billing period (no partial refunds)
- Subscription changes (upgrades) prorated and charged immediately

**Dependencies:**
- FR-TNT-004: Subscription tier management
- FR-PAY-001: Stripe integration

**Technical Notes:**
- Use Stripe Subscription API
- Store Stripe customer ID and subscription ID in Tenant table
- Webhook handling for subscription events: invoice.paid, invoice.payment_failed, customer.subscription.deleted
- Background job to enforce tier limits based on subscription status
- Implement grace period logic before downgrading failed payments

---

## 3.6 Domain: Identity and Access Management

### Overview
Identity and Access Management covers user authentication, authorization, role-based access control (RBAC), and tenant-scoped permissions.

---

### FR-IDN-001: Merchant Authentication (IdentityServer)

**Priority:** Must Have (MVP)

**Description:**
Merchants must be able to securely authenticate to the admin dashboard using email and password, with JWT tokens issued by IdentityServer for API access.

**User Story:**
```
Given I am a registered merchant
When I log in with my email and password
Then I receive a JWT token with my tenant and role claims
And I can access the admin dashboard and APIs
```

**Acceptance Criteria:**
- Login page with email and password fields
- Email verification required before first login
- Password requirements: min 8 chars, uppercase, lowercase, number, special character
- Failed login attempts tracked (max 5 within 15 minutes triggers account lockout for 30 minutes)
- Successful login returns JWT access token (expires in 1 hour) and refresh token (expires in 7 days)
- JWT includes claims: userId, email, tenantId, roles
- Logout invalidates refresh token
- "Forgot Password" flow sends reset link via email

**Business Rules:**
- One account per email address
- Email case-insensitive for login
- Account lockout after 5 failed attempts (security)
- Password reset link expires in 1 hour
- Passwords hashed using ASP.NET Core Identity defaults (PBKDF2)

**Dependencies:**
- IdentityServer setup (01_STEPS.md)
- Email service for verification and password reset

**Technical Notes:**
- ASP.NET Core Identity for user storage
- IdentityServer4 for token issuance (OpenID Connect + OAuth2)
- Authorization Code with PKCE flow for Angular SPA
- Store IdentityUser with TenantId custom claim
- Implement IUserClaimsPrincipalFactory to inject TenantId claim
- Use HTTP-only cookies for refresh tokens (security)

---

### FR-IDN-002: Role-Based Access Control (RBAC)

**Priority:** Must Have (MVP)

**Description:**
The platform must enforce role-based access control with predefined roles (MerchantAdmin, MerchantStaff, PlatformAdmin) and tenant-scoped permissions.

**User Story:**
```
Given I am a MerchantStaff user
When I attempt to access subscription settings
Then I am denied access because only MerchantAdmin can manage subscriptions
```

**Acceptance Criteria:**
- Roles defined: MerchantAdmin, MerchantStaff, PlatformAdmin, Customer (future)
- MerchantAdmin permissions: full access to store settings, products, orders, users, subscription
- MerchantStaff permissions: manage products, view/update orders (cannot change subscription or settings)
- PlatformAdmin permissions: access all tenants, view platform analytics, manage global settings
- Role assignment during user creation
- Role checked via [Authorize(Roles = "...")] attributes on API endpoints
- Unauthorized access returns 403 Forbidden

**Business Rules:**
- Each tenant must have at least one MerchantAdmin
- Cannot delete last MerchantAdmin from tenant
- PlatformAdmin is global (not tenant-scoped)
- Roles are tenant-scoped except PlatformAdmin

**Dependencies:**
- FR-IDN-001: Authentication
- ASP.NET Core Identity Roles

**Technical Notes:**
- Use ASP.NET Core Identity Roles
- Store tenant-scoped roles in AspNetUserRoles with TenantId
- Custom authorization policy for tenant isolation
- Middleware to validate TenantId claim matches resource TenantId

---

### FR-IDN-003: Staff User Management

**Priority:** Should Have (MVP)

**Description:**
Merchant admins must be able to invite and manage staff users with limited permissions.

**User Story:**
```
Given I am a MerchantAdmin
When I invite a staff member with their email
Then they receive an invitation email with a signup link
And after signup, they can access the admin dashboard with limited permissions
```

**Acceptance Criteria:**
- Invite staff user by email (sends invitation link)
- Invitation link expires in 7 days
- Invited user sets password during signup
- Assign role during invitation (MerchantAdmin or MerchantStaff)
- View list of all staff users for tenant
- Deactivate/reactivate staff user accounts
- Cannot delete user with existing audit trail (soft delete only)
- Staff user limits per tier (Free: 1, Starter: 3, Pro: 10)

**Business Rules:**
- Invitation email unique per tenant (cannot reuse pending invitations)
- Deactivated users cannot log in but data retained
- Tier limits enforced (cannot invite beyond limit)

**Dependencies:**
- FR-IDN-002: RBAC
- FR-TNT-004: Tier limits
- Email service

**Technical Notes:**
- StaffInvitation table with token, email, tenantId, expiresAt
- Generate secure random token for invitation link
- Validate invitation token hasn't been used or expired
- Background job to clean up expired invitations (30 days old)

---

### FR-IDN-004: Platform Admin Dashboard

**Priority:** Should Have (MVP)

**Description:**
Platform administrators must have access to a global admin dashboard to monitor all tenants, view platform analytics, and perform support tasks.

**User Story:**
```
Given I am a PlatformAdmin
When I log into the platform admin dashboard
Then I see a list of all tenants with health metrics
And I can impersonate a merchant to troubleshoot issues
```

**Acceptance Criteria:**
- Platform admin login (separate from merchant login)
- Dashboard shows: total tenants, active tenants, total orders, total revenue, system health
- Tenant list with search and filters
- View individual tenant details (without exposing payment card data)
- Impersonate merchant (with audit log of impersonation events)
- View webhook failures and retry manually
- Access platform-wide logs and error reports

**Business Rules:**
- Only predefined PlatformAdmin users (seeded in database)
- All platform admin actions logged with user and timestamp
- Impersonation limited to 30-minute session
- Cannot modify tenant data without impersonation (read-only access)

**Dependencies:**
- FR-IDN-002: PlatformAdmin role
- FR-TNT-005: Tenant isolation (admin can override)

**Technical Notes:**
- Separate Angular app or route for platform admin
- Implement impersonation by generating tenant-scoped JWT
- Log impersonation events to AuditLog table
- Platform admin dashboard uses aggregate queries across all tenants
- Implement caching for platform-wide metrics

---

### FR-IDN-005: Multi-Factor Authentication (MFA)

**Priority:** Won't Have (MVP) - Post-MVP Feature

**Description:**
Merchant admins and platform admins should be able to enable MFA using authenticator apps (TOTP) for enhanced security.

**User Story:**
```
Given I am a MerchantAdmin
When I enable MFA in my account settings
Then I am required to enter a TOTP code on each login
```

**Acceptance Criteria:**
- Enable/disable MFA in user profile settings
- MFA setup shows QR code for authenticator app
- Backup codes generated for account recovery
- MFA required for PlatformAdmin (mandatory)
- MFA optional for MerchantAdmin and MerchantStaff (recommended)
- Login flow prompts for TOTP code after password verification

**Business Rules:**
- MFA mandatory for PlatformAdmin
- Cannot disable MFA for PlatformAdmin without support ticket
- Backup codes single-use (regenerate after use)

**Dependencies:**
- FR-IDN-001: Authentication

**Technical Notes:**
- Use ASP.NET Core Identity MFA support
- TOTP implementation using QRCoder library
- Store MFA secret encrypted in database
- Backup codes hashed like passwords

---

## 3.7 Domain: Platform Administration

### Overview
Platform Administration covers system-wide monitoring, configuration, merchant support tools, and operational management.

---

### FR-ADM-001: System Health Monitoring

**Priority:** Should Have (MVP)

**Description:**
The platform must provide real-time health monitoring with alerts for critical issues like API failures, database connection issues, and payment webhook failures.

**User Story:**
```
Given I am a PlatformAdmin
When the database connection fails
Then I receive an immediate alert via email
And the health dashboard shows the service as unhealthy
```

**Acceptance Criteria:**
- Health check endpoints for all services (return HTTP 200 if healthy)
- Health dashboard shows: API status, database status, payment provider status, webhook processing status
- Automated health checks every 1 minute
- Alert on: API response time > 2 seconds, database connection failure, > 10% webhook failure rate, disk space < 10%
- Alerts sent via email to PlatformAdmin
- Health status history for last 30 days

**Business Rules:**
- Health check endpoint is unauthenticated (public) for uptime monitoring tools
- Alerts deduplicated (max 1 alert per issue per hour)

**Dependencies:**
- Application Insights or monitoring service

**Technical Notes:**
- Implement ASP.NET Core Health Checks
- Health check includes: database ping, Redis cache ping, payment provider API test call
- Use Application Insights for metrics and alerting
- Store health check results in time-series database for historical analysis

---

### FR-ADM-002: Platform Analytics Dashboard

**Priority:** Could Have (Post-MVP)

**Description:**
Platform admins should have access to analytics showing platform growth, usage trends, and business metrics.

**User Story:**
```
Given I am a PlatformAdmin
When I view the analytics dashboard
Then I see total merchants, order volume, revenue, and growth trends
```

**Acceptance Criteria:**
- Metrics: total tenants (active/inactive), total orders, gross merchandise volume (GMV), MRR, churn rate
- Charts: tenant growth over time, order volume over time, revenue over time
- Filters: date range, tier (Free/Starter/Pro)
- Export reports to CSV
- Refresh metrics daily (not real-time for MVP)

**Business Rules:**
- Analytics exclude deleted tenants
- Revenue metrics exclude refunds

**Dependencies:**
- FR-TNT-001: Tenant data
- FR-ORD-003: Order data
- FR-PAY-005: Subscription data

**Technical Notes:**
- Aggregate queries with database views or materialized tables
- Background job to calculate daily metrics
- Consider data warehouse for analytics (post-MVP)
- Use Chart.js or similar for visualization

---

### FR-ADM-003: Merchant Support Tools

**Priority:** Should Have (MVP)

**Description:**
Platform admins must have tools to support merchants including the ability to search orders across all tenants, view merchant details, and manually resolve payment issues.

**User Story:**
```
Given I am a PlatformAdmin helping a merchant
When I search for their order number
Then I can view the order details and payment status across all tenants
And I can manually mark a payment as successful if needed
```

**Acceptance Criteria:**
- Global search: search orders by order number, customer email (across all tenants)
- View merchant details: contact info, subscription tier, usage stats
- Manual payment reconciliation: mark payment as Success/Failed with reason
- Manually trigger webhook reprocessing
- View audit log for specific merchant actions

**Business Rules:**
- All support actions logged with PlatformAdmin user and timestamp
- Manual payment changes require reason field (mandatory)
- Cannot delete orders or payments (read-only except status updates)

**Dependencies:**
- FR-IDN-004: Platform admin authentication
- FR-ORD-004: Order search

**Technical Notes:**
- Global search queries across all tenants (remove TenantId filter)
- Implement admin action audit log (AdminAuditLog table)
- Manual payment status change triggers same order update logic as webhook

---

### FR-ADM-004: Email Template Management

**Priority:** Could Have (Post-MVP)

**Description:**
Platform admins should be able to manage email templates for transactional emails (order confirmation, shipping notification, password reset).

**User Story:**
```
Given I am a PlatformAdmin
When I update the order confirmation email template
Then all merchants use the updated template for new orders
```

**Acceptance Criteria:**
- Email templates: order confirmation, shipping notification, password reset, low stock alert
- Template editor with HTML support and variable placeholders (e.g., {{orderNumber}}, {{customerName}})
- Preview template with sample data
- Versioning: save template history
- Rollback to previous version if needed

**Business Rules:**
- Template changes apply to all merchants globally
- Cannot delete templates (only deactivate)
- HTML sanitized to prevent XSS

**Dependencies:**
- Email service integration

**Technical Notes:**
- Store templates in EmailTemplate table
- Use Liquid or Handlebars for template rendering
- Sanitize HTML using HtmlSanitizer library
- Cache templates in Redis for performance

---

# 4. Key Business Questions & Answers

## 4.1 Target Merchant Profile

**Question:** Who is the ideal merchant for the Vendo MVP?

**Answer:**

**Primary Target: Small to Medium Businesses (SMBs) in the United States**

### Ideal Merchant Characteristics

1. **Size & Revenue:**
   - 1-10 employees (solopreneur to small team)
   - $100K - $1M annual revenue
   - 50-500 products in catalog
   - 50-500 orders per month

2. **Current Situation:**
   - Currently selling on marketplaces (Etsy, Amazon, eBay) or social media
   - Frustrated with high marketplace fees (6-15% of sales)
   - Wants to build independent brand presence
   - Lacks technical expertise to manage WordPress/WooCommerce
   - Budget-conscious but willing to invest $25-100/month for right solution

3. **Industry Verticals (Priority Order):**
   - **High Priority:** Handmade/artisan goods, apparel/fashion, health/beauty
   - **Medium Priority:** Digital products, niche consumer goods, food/beverage
   - **Low Priority (Future):** Services, B2B wholesale, high-value items requiring complex workflows

4. **Technical Profile:**
   - Comfortable with web-based software (social media, Etsy, email)
   - Uncomfortable with code, hosting, server management
   - Expects intuitive UI similar to modern SaaS tools
   - Needs onboarding help and documentation

5. **Business Goals:**
   - Own customer relationships and data
   - Reduce dependency on marketplaces (diversification)
   - Build email marketing list
   - Control brand presentation and customer experience
   - Grow revenue while controlling costs

### Why This Target for MVP?

1. **Market Size:** 1.8M small online sellers in US (SBA data)
2. **Pain Point Intensity:** High marketplace fees create strong motivation to switch
3. **Low Complexity:** Simple product catalogs, straightforward fulfillment
4. **Validation Speed:** Can onboard and validate quickly (weeks, not months)
5. **Expansion Path:** Successful SMBs grow into higher tiers or refer peers

### Merchants to Avoid (MVP)

- Enterprise merchants (complex requirements, long sales cycles)
- Multi-warehouse/multi-location operations
- Complex B2B with custom pricing, quotes, approval workflows
- Dropshipping-only businesses (different feature needs)
- High-volume merchants (>10K orders/month) requiring advanced performance

---

## 4.2 Geographic Market Focus

**Question:** What geographic markets will Vendo serve, and in what order?

**Answer:**

### Phase 1: United States Only (MVP - Months 0-6)

**Rationale:**
- Largest e-commerce market globally ($870B in 2023)
- Single regulatory framework (compared to EU's 27 countries)
- Homogeneous payment processing (USD, established providers)
- Native language (English) reduces localization complexity
- Founder/team market knowledge and timezone alignment

**MVP Constraints:**
- Currency: USD only
- Payment providers: US-based Stripe accounts
- Shipping: US domestic addresses only
- Tax calculation: Simple percentage-based (no automatic tax API initially)
- Language: English only
- Compliance: US tax laws, basic GDPR-ready architecture

**Success Metrics for US Market:**
- 20+ active merchants by month 6
- $10K MRR by month 12
- 95%+ payment success rate
- NPS > 40

---

### Phase 2: English-Speaking Markets (Months 6-12)

**Target Markets:**
1. **Canada**
   - Currency: CAD support
   - Payment: Stripe Canada
   - Shipping: Canada Post integration
   - Tax: Basic GST/PST calculation

2. **United Kingdom**
   - Currency: GBP support
   - Payment: Stripe UK
   - Shipping: Royal Mail integration
   - Tax: VAT calculation

3. **Australia**
   - Currency: AUD support
   - Payment: Stripe Australia
   - Shipping: Australia Post
   - Tax: GST calculation

**Why These Markets:**
- English language (minimal localization)
- Cultural similarity to US market
- Established Stripe presence
- Combined market size adds ~$150B e-commerce
- Similar merchant pain points (marketplace fee frustration)

**Requirements:**
- Multi-currency support in catalog and checkout
- Currency conversion and display
- Country-specific payment provider accounts
- Localized tax calculation templates
- International shipping rate calculations

---

### Phase 3: European Union (Months 12-24)

**Target Markets (Priority):**
1. Germany (largest EU e-commerce market)
2. France
3. Spain
4. Italy
5. Netherlands

**Requirements:**
- **GDPR Compliance (Full):**
  - Data subject access requests (export personal data)
  - Right to erasure (delete customer data)
  - Data processing agreements (DPA) for merchants
  - Cookie consent management
  - Privacy policy templates

- **Multi-Language Support:**
  - German, French, Spanish, Italian, Dutch
  - Admin dashboard localization
  - Storefront templates
  - Email templates

- **VAT Compliance:**
  - VAT number validation
  - Cross-border VAT rules
  - OSS (One-Stop-Shop) integration
  - Digital goods VAT rates

- **Payment Providers:**
  - Stripe EU accounts
  - Local payment methods (Sofort, iDEAL, Bancontact, Giropay)

**Challenges:**
- 27 different tax regimes (high complexity)
- GDPR enforcement risk (€20M or 4% of global revenue fines)
- Language localization costs (translation, maintenance)
- Cultural differences in UX expectations
- Customer support in multiple languages

**Go/No-Go Criteria:**
- US market achieving $25K+ MRR (validates product-market fit)
- Team capacity to support multiple languages
- Legal review of GDPR compliance
- Partnership with EU-based payment processors

---

### Phase 4: Asia-Pacific (18-36 Months)

**Potential Markets:**
- Singapore (English-speaking, business-friendly)
- Japan (large market, high e-commerce adoption)
- India (growing SMB market, English-speaking)

**Not Planned:**
- China (requires separate platform, regulatory complexity)
- Middle East (future consideration)
- Latin America (future consideration)

---

### GDPR-Ready Architecture (MVP Requirement)

Even though MVP focuses on US, architecture must be GDPR-ready for future EU expansion:

**Design Principles:**
1. **Data Minimization:** Only collect essential customer data
2. **Purpose Limitation:** Use data only for stated purposes (order fulfillment)
3. **Storage Limitation:** Define retention periods, auto-delete old data
4. **Portability:** Export customer data in machine-readable format (JSON/CSV)
5. **Erasure:** Soft delete customers with ability to hard delete (purge)
6. **Consent Management:** Track consent for marketing emails separately from transactional
7. **Data Processing Agreements:** Template DPA for merchants

**MVP Implementation:**
- Customer data export API endpoint (FR-CUS-005 - future requirement)
- Customer deletion API with cascade rules (FR-CUS-006 - future requirement)
- Audit log of data access and modifications
- Encryption at rest for PII fields
- Data retention policy documented (07_SECURITY_AND_COMPLIANCE.md)

**Post-MVP:**
- Cookie consent banner for EU visitors
- GDPR-specific privacy policy generator for merchants
- Data processing impact assessment (DPIA) templates
- DPA signing workflow

---

## 4.3 MVP Feature Set Definition

**Question:** What is the minimum feature set to deliver a viable product that merchants will pay for?

**Answer:**

### MVP Core Features (Must Have - Month 0-4)

The MVP must enable a merchant to **launch a functional online store and process their first sale within 2 hours of signup**.

#### 1. Store Setup & Configuration (Week 1-2)
- Self-service merchant registration (FR-TNT-001)
- Store settings: name, contact info, currency, tax rate (FR-TNT-002)
- Basic branding: logo upload, primary color (FR-TNT-003)
- Free tier active by default

**Validation:** Merchant can create store and configure branding in < 30 minutes

---

#### 2. Product Catalog (Week 3-5)
- Create products with name, description, price, images (FR-CAT-001)
- Product variants: size, color (max 3 variant types) (FR-CAT-003)
- Basic inventory tracking with stock quantity (FR-CAT-005)
- Product categories for organization (FR-CAT-002)
- Publish/draft status workflow
- Product image upload and optimization (FR-CAT-004)

**Validation:** Merchant can list 50 products with variants in < 2 hours

**MVP Constraints:**
- No bulk import (manual entry only)
- No advanced SEO fields
- No product reviews/ratings
- No related products/upsells

---

#### 3. Storefront (Week 6-8)
- Public storefront at {store}.vendo.app subdomain
- Product listing page with category filter
- Product detail page with variant selection
- Basic search by product name
- Shopping cart with add/remove/update quantity
- Mobile-responsive design (Tailwind CSS)

**Validation:** Customers can browse products and add to cart on desktop and mobile

**MVP Constraints:**
- Single default theme (no theme customization)
- No custom domain support
- No advanced filtering (price range, multi-select)
- No customer reviews or wishlists

---

#### 4. Checkout & Payments (Week 9-11)
- Guest checkout (no account creation required)
- Shipping information form
- Order review with totals (subtotal, tax, shipping, total)
- Stripe Checkout hosted payment integration (FR-PAY-001)
- Order confirmation page and email
- Payment webhook handling (FR-PAY-004)
- Free shipping (no shipping rate calculation)

**Validation:** Customers can complete end-to-end purchase in < 3 minutes

**MVP Constraints:**
- USD currency only
- Stripe only (no PayPal, Square)
- Free shipping (no rate calculation or carrier integration)
- No discount codes or promotions
- No abandoned cart recovery

---

#### 5. Order Management (Week 12-13)
- Order dashboard with list view (FR-ORD-003)
- Order detail view with customer info, items, status
- Order status workflow: Pending → Paid → Shipped → Delivered
- Mark order as shipped with tracking number
- Order search by number, customer email (FR-ORD-004)
- Customer order confirmation email
- Customer shipping notification email

**Validation:** Merchant can fulfill 10 orders in < 15 minutes

**MVP Constraints:**
- No bulk order operations
- No automatic carrier tracking updates
- No refund processing (manual via Stripe dashboard)
- No order notes or internal messaging

---

#### 6. Authentication & Users (Week 14-15)
- Merchant login with email/password (FR-IDN-001)
- Email verification on signup
- Password reset flow
- Basic RBAC: MerchantAdmin role (FR-IDN-002)
- Session management with JWT tokens

**Validation:** Secure authentication with industry-standard security

**MVP Constraints:**
- No staff user invitations (single merchant admin only)
- No MFA (post-MVP security enhancement)
- No social login (Google, Facebook)

---

#### 7. Subscription Billing (Week 16)
- Three tiers: Free, Starter ($29/mo), Pro ($79/mo)
- Tier limits enforced: products, orders, users (FR-TNT-004)
- Self-service upgrade via Stripe Billing (FR-PAY-005)
- Billing history and invoice access
- 14-day Pro trial for new signups

**Validation:** Merchants can upgrade and be charged successfully

**MVP Constraints:**
- No annual billing option (monthly only)
- No enterprise custom plans
- No promo codes for subscriptions

---

### MVP Feature Exclusions (Post-MVP)

These features are intentionally excluded from MVP to focus on core value delivery:

#### Customer Features (Post-MVP)
- Customer accounts and login
- Order history for customers
- Wishlists and saved carts
- Product reviews and ratings
- Loyalty programs

#### Marketing & Sales (Post-MVP)
- Discount codes and promotions
- Gift cards
- Abandoned cart recovery emails
- Email marketing campaigns
- Affiliate program

#### Advanced Catalog (Post-MVP)
- Bulk product import/export (FR-CAT-007)
- SEO optimization fields
- Related products and upsells
- Product bundles
- Digital product delivery

#### Advanced Operations (Post-MVP)
- Multi-warehouse inventory
- Advanced shipping rate calculation
- Carrier integrations (ShipStation, EasyPost)
- Automated tax calculation (TaxJar, Avalara)
- Multi-currency display

#### Analytics & Reporting (Post-MVP)
- Advanced analytics dashboard
- Sales reports and trends
- Customer lifetime value
- Inventory forecasting
- Export reports

#### Platform Features (Post-MVP)
- Custom domains (FR-TNT-006)
- Multi-language support
- Theme customization
- Plugin/app marketplace
- Webhooks for third-party integrations

---

### MVP Success Criteria

**Merchant Success:**
- Time to first sale: < 4 hours from signup to first paid order
- Setup abandonment: < 30% (70%+ complete setup)
- First-month retention: > 60%

**Technical Success:**
- System uptime: 99.5%
- Payment success rate: > 95%
- Page load time: < 2 seconds (p95)
- Zero critical security vulnerabilities

**Business Success:**
- 20 active merchants by month 6
- 5 paying merchants (Starter/Pro) by month 6
- $500+ MRR by month 6
- NPS > 40

---

## 4.4 Pricing Strategy

**Question:** What pricing model and tiers will Vendo use to monetize while remaining competitive with Shopify and WooCommerce?

**Answer:**

### Pricing Model: Tiered Subscription (SaaS)

**Core Principle:** Predictable monthly subscription with no transaction fees (beyond payment processor costs)

**Why This Model:**
- Predictable revenue for platform (MRR growth)
- Predictable costs for merchants (no surprise fees as they grow)
- Competitive differentiation vs. Shopify (no transaction fees)
- Aligns incentives (we succeed when merchants succeed, not by taxing transactions)

---

### Three-Tier Structure

| Feature | Free | Starter | Pro |
|---------|------|---------|-----|
| **Monthly Price** | $0 | $29 | $79 |
| **Annual Price** (post-MVP) | $0 | $290 ($24.17/mo, save 17%) | $790 ($65.83/mo, save 17%) |
| | | | |
| **Core Limits** | | | |
| Products | 50 | 500 | Unlimited |
| Orders per month | 100 | 1,000 | Unlimited |
| Storage | 1 GB | 10 GB | 100 GB |
| Admin users | 1 | 3 | 10 |
| | | | |
| **Features** | | | |
| Custom branding (logo, colors) | ✓ | ✓ | ✓ |
| Payment processing (Stripe) | ✓ | ✓ | ✓ |
| SSL certificate | ✓ | ✓ | ✓ |
| Basic analytics | ✓ | ✓ | ✓ |
| Email support | - | ✓ | ✓ |
| Priority support | - | - | ✓ |
| | | | |
| **Advanced Features** | | | |
| Advanced analytics & reports | - | ✓ | ✓ |
| Discount codes & promotions | - | ✓ | ✓ |
| Abandoned cart recovery | - | - | ✓ |
| Custom domain | - | - | ✓ |
| API access (future) | - | - | ✓ |
| Remove "Powered by Vendo" | - | - | ✓ |
| | | | |
| **Transaction Fees** | 0% | 0% | 0% |
| **Payment processing** | Stripe fees (2.9% + 30¢) | Stripe fees (2.9% + 30¢) | Stripe fees (2.9% + 30¢) |

---

### Tier Details

#### Free Tier - "Get Started"

**Target:** Brand new merchants, hobbyists, testing the platform

**Value Proposition:** "Start selling online for free - upgrade when you grow"

**Limits Rationale:**
- 50 products: Enough for small artisan shops, forces upgrade as catalog grows
- 100 orders/month: ~3 orders/day, adequate for early-stage businesses
- 1 GB storage: ~200-300 product images, enough for small catalogs
- 1 admin user: Solo merchant, need team = upgrade

**Monetization Goal:** Free tier as marketing funnel
- Convert 20% to Starter within 3 months
- Convert 5% to Pro within 6 months

**Why Offer Free Tier:**
- Lowers barrier to entry (compete with free WooCommerce)
- Builds user base for word-of-mouth growth
- Merchants "grow into" paid tiers naturally
- Platform costs low for small merchants (minimal infrastructure per tenant)

**Constraints:**
- Displays "Powered by Vendo" badge in storefront footer
- Email support not included (self-service docs only)
- Limited to 3 color customizations per month

---

#### Starter Tier - "Grow Your Business" ($29/month)

**Target:** Growing merchants with consistent sales, small teams

**Value Proposition:** "Everything you need to run a professional store - less than a dollar a day"

**Limits Rationale:**
- 500 products: Supports most SMB catalogs
- 1,000 orders/month: ~33 orders/day, covers merchants up to ~$50K/month revenue
- 10 GB storage: ~2,000-3,000 product images
- 3 admin users: Small team (owner + 2 staff)

**Feature Additions:**
- Advanced analytics: Sales trends, top products, customer insights
- Discount codes: Essential for marketing campaigns
- Email support: Response within 24 business hours

**Price Justification:**
- **vs. Shopify Basic ($39/mo):** $10/month cheaper, no transaction fees (save $29-$145/mo on $1K-$5K sales)
- **vs. WooCommerce:** Similar price for managed hosting, but includes everything (no plugin costs)
- **vs. Square Online ($29/mo):** More features, better extensibility

**Expected Conversion:**
- Free → Starter: When hitting product limit (50→500) or order volume
- Starter → Pro: When hitting order limit or needing advanced features

**Target Mix:** 60% of paying merchants on this tier

---

#### Pro Tier - "Scale Without Limits" ($79/month)

**Target:** Established merchants, growing brands, power users

**Value Proposition:** "Unlimited growth potential with enterprise features"

**Limits:**
- Unlimited products: No catalog constraints
- Unlimited orders: Predictable costs as you scale
- 100 GB storage: 20,000+ images
- 10 admin users: Full team support

**Feature Additions:**
- Abandoned cart recovery: Recover 10-15% of abandoned carts (ROI: $500-$5K/month)
- Custom domain: Professional branding (shop.yourbrand.com)
- Priority support: Response within 4 business hours
- API access (future): Enable custom integrations and automation

**Price Justification:**
- **vs. Shopify Advanced ($399/mo):** Massive savings ($320/month)
- **vs. Shopify Basic ($39/mo) with transaction fees:** Break-even at ~$2K/month sales (saves $58/month on transaction fees, pays extra $40 for unlimited features)
- **vs. BigCommerce Standard ($29/mo):** More expensive but truly unlimited (BigCommerce has revenue limits per tier)

**ROI Pitch:**
- Abandoned cart recovery alone can recover $500+/month (6.3x ROI)
- Custom domain increases conversion 15-30% (increased trust)
- API access enables workflow automation (save 5-10 hours/month)

**Target Mix:** 40% of paying merchants on this tier

---

### Competitive Comparison

| Platform | Entry Price | Transaction Fees | Notes |
|----------|-------------|------------------|-------|
| **Vendo Free** | $0 | 0% (+ Stripe 2.9%) | 50 products, 100 orders/mo |
| **Vendo Starter** | $29/mo | 0% (+ Stripe 2.9%) | 500 products, 1K orders/mo |
| **Vendo Pro** | $79/mo | 0% (+ Stripe 2.9%) | Unlimited |
| **Shopify Basic** | $39/mo | 2% (or 0% with Shopify Payments) | Transaction fee unless using Shopify Payments |
| **Shopify** | $105/mo | 1% (or 0% with Shopify Payments) | More expensive, same transaction fee issue |
| **WooCommerce** | $0-30/mo (hosting) | 0% (+ payment processor fees) | Requires managing hosting, security, plugins ($100+/year) |
| **Square Online** | $29/mo | 2.9% + 30¢ | Limited features, less customizable |
| **BigCommerce Standard** | $29/mo | 0% | $50K annual sales limit on Standard plan |

**Vendo Advantage:**
- **vs. Shopify:** $10-26/month cheaper + no transaction fees (saves $20-$200+/month depending on sales)
- **vs. WooCommerce:** Managed (no hosting headaches) + predictable costs (no plugin surprise fees)
- **vs. BigCommerce:** More expensive but no revenue limits, better for growth

---

### Pricing Strategy Rationale

#### 1. No Transaction Fees
- **Merchant Benefit:** Costs predictable and don't penalize success (unlike Shopify's 2%)
- **Competitive Moat:** Clear differentiator in messaging ("We grow when you grow, not by taxing your sales")
- **Business Benefit:** Simpler accounting, no disputes over transaction tracking

#### 2. Generous Free Tier
- **Customer Acquisition:** Low-friction onboarding, high volume of signups
- **Word of Mouth:** Free users become advocates
- **Upgrade Path:** Natural conversion as merchants grow
- **Risk Mitigation:** Some free merchants never upgrade (acceptable acquisition cost)

#### 3. Starter Tier Sweet Spot ($29)
- **Psychological Pricing:** Under $30/month feels affordable ("less than a dollar a day")
- **Market Positioning:** Match Square Online, undercut Shopify
- **Value Perception:** Advanced features justify $29 vs. $0

#### 4. Pro Tier Value Focus ($79)
- **ROI Justification:** Abandoned cart recovery alone justifies cost
- **Unlimited Safety:** "Never worry about outgrowing your plan"
- **Premium Positioning:** Not trying to be cheapest, trying to be best value

#### 5. Annual Discount (Post-MVP)
- **Cash Flow:** Upfront annual payments improve runway
- **Retention:** Annual commitments reduce churn
- **Standard Discount:** 17% (~2 months free) is industry standard

---

### Revenue Projections (Illustrative)

**Assumptions:**
- Total merchants: 100 active by Month 12
- Tier distribution: 30% Free, 40% Starter, 30% Pro

**Month 12 MRR:**
- Free: 30 merchants × $0 = $0
- Starter: 40 merchants × $29 = $1,160
- Pro: 30 merchants × $79 = $2,370
- **Total MRR: $3,530**
- **ARR: $42,360**

**Conservative Scenario (Lower Conversion):**
- 50% Free, 35% Starter, 15% Pro = $1,200 MRR

**Optimistic Scenario (Higher Conversion):**
- 20% Free, 40% Starter, 40% Pro = $4,320 MRR

**Target: $10K MRR by Month 18**
- Requires ~285 total merchants with 30/40/30 split
- Or ~200 merchants with aggressive conversion to Pro

---

### Future Pricing Considerations (Post-MVP)

#### 1. Enterprise Tier (Month 12+)
- **Price:** $299/month or custom
- **Target:** Merchants doing $500K+ annually
- **Features:** Dedicated account manager, custom integrations, SLA guarantees, white-label options

#### 2. Usage-Based Add-Ons (Month 18+)
- Additional storage: $10/month per 50 GB
- Additional admin users: $5/month per user
- SMS notifications: Pay-as-you-go ($0.02 per SMS)

#### 3. Transaction Fee Model (Optional)
- Alternative pricing: Lower/no subscription, higher transaction fees (1-2%)
- Target: High-volume, low-margin merchants who prefer variable costs
- Example: $0/month + 1% transaction fee

#### 4. Partner/Affiliate Tier
- **Price:** $0/month
- **Target:** Agencies, consultants building stores for clients
- **Commission:** 20% of merchant subscription revenue for first 12 months

---

### Pricing Communication Strategy

**Key Messages:**
1. **Transparent & Fair:** "No hidden fees. No transaction fees. No surprises."
2. **Grow With You:** "Start free, upgrade when you're ready."
3. **Better Value:** "Shopify features at WooCommerce prices - without the hassle."
4. **Predictable:** "Know your costs every month. Never worry about percentage cuts on your hard-earned sales."

**Pricing Page Best Practices:**
- Feature comparison table (Free vs. Starter vs. Pro)
- Calculator: "How much you save vs. Shopify" (input monthly sales, show savings)
- FAQ addressing: "Why no transaction fees?", "Can I switch tiers anytime?", "What happens if I exceed limits?"
- Social proof: Testimonials from merchants who saved money switching to Vendo

---

## 4.5 Go-to-Market Strategy (Brief Overview)

**Question:** How will Vendo acquire its first 20-100 merchants?

**Answer:**

### Phase 1: Pilot Program (Months 0-3)

**Goal:** 10-20 pilot merchants providing feedback

**Tactics:**
1. **Direct Outreach:** Personal network, Etsy sellers, artisan communities
2. **Free Pro Tier:** Offer 6-month free Pro in exchange for feedback
3. **Case Studies:** Document success stories for future marketing

---

### Phase 2: Content Marketing (Months 3-9)

**Goal:** Organic traffic and brand awareness

**Tactics:**
1. **SEO Blog:** "Shopify alternatives", "WooCommerce vs. Managed Platforms", "How to start online store"
2. **Comparison Pages:** "Vendo vs. Shopify", "Vendo vs. WooCommerce"
3. **Guides:** "Complete Guide to Launching Your Etsy Store Alternative"

---

### Phase 3: Paid Acquisition (Months 6-12)

**Goal:** Scalable customer acquisition

**Tactics:**
1. **Google Ads:** Target "Shopify alternatives", "e-commerce platform"
2. **Facebook/Instagram Ads:** Target Etsy sellers, small business owners
3. **Partnerships:** Affiliate deals with web agencies, business consultants

**Target CAC:** < $200 (pays back in < 7 months on Starter tier)

---

### Phase 4: Ecosystem & Referrals (Months 12+)

**Goal:** Viral growth and platform network effects

**Tactics:**
1. **Referral Program:** Give 1 month free for each referral
2. **Agency Partnerships:** Revenue share with agencies who build stores
3. **App Marketplace:** Third-party developers extend platform

---

## 4.6 Risk Analysis & Mitigation

**Question:** What are the biggest risks to Vendo's success, and how will they be mitigated?

**Answer:**

### Risk 1: Low Product-Market Fit (High Impact, Medium Probability)

**Risk:** Merchants don't find enough value to pay/stay

**Indicators:**
- High churn rate (> 15% monthly)
- Low free-to-paid conversion (< 10%)
- Poor NPS (< 20)

**Mitigation:**
- Continuous merchant interviews (2-3 per week)
- Feature prioritization based on merchant feedback
- Fast iteration cycles (ship improvements weekly)
- Pilot program with close merchant relationships

---

### Risk 2: Technical Debt / Quality Issues (Medium Impact, Medium Probability)

**Risk:** Rushing to MVP creates unstable, hard-to-maintain codebase

**Indicators:**
- Increasing bug reports
- Slowing feature velocity
- Developer frustration

**Mitigation:**
- TDD enforcement from day one (FR in 06_TESTING_STRATEGY.md)
- Code review discipline
- Refactoring sprints scheduled proactively
- Technical debt tracking and paydown plan

---

### Risk 3: Payment Processing Failures (High Impact, Low Probability)

**Risk:** Webhook failures, payment provider outages cause lost revenue for merchants

**Indicators:**
- Payment success rate < 95%
- Increasing webhook failure alerts

**Mitigation:**
- Comprehensive webhook testing (FR-PAY-004)
- Retry logic and manual reconciliation tools (FR-ADM-003)
- Multiple payment provider support roadmap (FR-PAY-003 post-MVP)
- Real-time monitoring and alerts (FR-ADM-001)

---

### Risk 4: Security Breach / Data Leak (Catastrophic Impact, Low Probability)

**Risk:** Tenant data exposed due to security vulnerability

**Indicators:**
- Penetration test failures
- Security audit findings

**Mitigation:**
- Security-first architecture (07_SECURITY_AND_COMPLIANCE.md)
- Mandatory tenant isolation testing (FR-TNT-005)
- Regular security audits (quarterly post-MVP)
- Incident response plan (07_SECURITY_AND_COMPLIANCE.md)

---

### Risk 5: Competitive Response (Medium Impact, Medium Probability)

**Risk:** Shopify/WooCommerce copy pricing or features, negating differentiation

**Indicators:**
- Competitor pricing changes
- Feature parity erosion

**Mitigation:**
- Build features competitors can't easily copy (API-first, extensibility)
- Focus on customer experience, not just features
- Build loyal community and brand
- Fast innovation cadence (ship faster than competitors)

---

### Risk 6: Solo Developer Burnout / Key Person Risk (High Impact, Medium Probability)

**Risk:** Single developer becomes bottleneck or burns out

**Indicators:**
- Declining code commits
- Missed deadlines
- Developer stress/health issues

**Mitigation:**
- Realistic roadmap (don't overpromise)
- Automated testing and CI/CD to reduce manual burden
- Documentation for future team members
- Prioritize ruthlessly (focus on must-haves, cut nice-to-haves)
- Plan first hire at $10K MRR milestone

---

# Appendices

## Appendix A: Requirement Traceability Matrix

| Requirement ID | User Story | Priority | Domain | Estimated Effort (Days) |
|---------------|------------|----------|--------|------------------------|
| FR-TNT-001 | Merchant registration | Must Have | Tenant | 3 |
| FR-TNT-002 | Store settings | Must Have | Tenant | 2 |
| FR-TNT-003 | Branding | Should Have | Tenant | 3 |
| FR-TNT-004 | Subscription tiers | Must Have | Tenant | 5 |
| FR-TNT-005 | Tenant isolation | Must Have | Tenant | 5 |
| FR-CAT-001 | Product CRUD | Must Have | Catalog | 5 |
| FR-CAT-002 | Categories | Must Have | Catalog | 3 |
| FR-CAT-003 | Product variants | Must Have | Catalog | 5 |
| FR-CAT-004 | Image management | Should Have | Catalog | 4 |
| FR-CAT-005 | Inventory | Must Have | Catalog | 4 |
| FR-CAT-006 | Search/filter | Should Have | Catalog | 3 |
| FR-ORD-001 | Shopping cart | Must Have | Orders | 4 |
| FR-ORD-002 | Checkout | Must Have | Orders | 6 |
| FR-ORD-003 | Order management | Must Have | Orders | 5 |
| FR-ORD-004 | Order search | Should Have | Orders | 2 |
| FR-PAY-001 | Stripe integration | Must Have | Payments | 6 |
| FR-PAY-002 | Payment tracking | Must Have | Payments | 3 |
| FR-PAY-004 | Webhook security | Must Have | Payments | 4 |
| FR-PAY-005 | Subscription billing | Must Have | Payments | 5 |
| FR-IDN-001 | Authentication | Must Have | Identity | 4 |
| FR-IDN-002 | RBAC | Must Have | Identity | 3 |
| FR-IDN-003 | Staff management | Should Have | Identity | 4 |
| FR-IDN-004 | Platform admin | Should Have | Identity | 5 |
| FR-ADM-001 | Health monitoring | Should Have | Admin | 3 |
| FR-ADM-003 | Support tools | Should Have | Admin | 4 |

**Total Estimated Effort (Must Have + Should Have): ~95 days**

**Assumes:** Single developer, includes testing and documentation time

**Timeline:** ~4 months with buffer for unknowns

---

## Appendix B: Key Terms & Definitions

| Term | Definition |
|------|------------|
| **Tenant** | A merchant store instance with isolated data and configuration |
| **TenantId** | Unique identifier for each merchant store (GUID) |
| **SKU** | Stock Keeping Unit - unique product identifier |
| **GMV** | Gross Merchandise Volume - total sales value processed through platform |
| **MRR** | Monthly Recurring Revenue - predictable subscription revenue |
| **Churn Rate** | Percentage of merchants who cancel per month |
| **NPS** | Net Promoter Score - customer satisfaction metric (-100 to +100) |
| **PCI DSS** | Payment Card Industry Data Security Standard - security requirements for handling card data |
| **GDPR** | General Data Protection Regulation - EU privacy law |
| **Hosted Checkout** | Payment page hosted by payment provider (Stripe, PayPal) to avoid PCI scope |
| **Webhook** | HTTP callback from external service (e.g., payment confirmation) |
| **JWT** | JSON Web Token - authentication token format |
| **RBAC** | Role-Based Access Control - permission system based on user roles |
| **Soft Delete** | Marking records as deleted without removing from database (IsActive = false) |
| **Clean Architecture** | Software design pattern separating concerns into layers (Domain, Application, Infrastructure, Presentation) |

---

## Appendix C: Document Change Log

| Date | Version | Section | Change Description | Author |
|------|---------|---------|-------------------|--------|
| 2025-10-19 | 1.0 | All | Initial document creation | Business Analyst |

---

## Appendix D: Next Steps & Handoff to Project Manager

### Immediate Next Steps

1. **Review & Validation (Week 1)**
   - Product Owner reviews and approves BRD Part 1
   - Stakeholder feedback session with pilot merchant candidates
   - Revisions based on feedback

2. **Handoff to Project Manager (Week 2)**
   - Project Manager reviews functional requirements
   - Identifies dependencies and sequencing constraints
   - Creates detailed sprint plan and backlog
   - Assigns story points to requirements

3. **UX/UI Handoff Preparation (Week 2-3)**
   - Business Analyst prepares wireframe requirements for UX team
   - User flow diagrams for key journeys (signup, product creation, checkout)
   - UX Engineer begins design phase

4. **Part 2 Creation (Week 3-4)**
   - Business Analyst creates BRD Part 2:
     - Non-Functional Requirements (performance, scalability, security)
     - Integration Requirements (payment providers, email, storage)
     - Data Models and Entity Relationships
     - API Contract Specifications
     - Detailed User Flows and Edge Cases

### Questions for Product Owner

1. **Pricing Validation:**
   - Confirm tier pricing ($0/$29/$79) and limits
   - Approve 14-day Pro trial for new signups

2. **MVP Scope Confirmation:**
   - Any features to add/remove from MVP must-haves?
   - Comfort level with 4-month timeline estimate?

3. **Market Strategy:**
   - Confirm US-only focus for MVP
   - Pilot merchant recruitment strategy and timeline

4. **Risk Acceptance:**
   - Review risk matrix - any additional risks to consider?
   - Acceptable risk tolerance levels?

### Handoff Artifacts for Project Manager

1. This BRD (Part 1) - Functional requirements and business context
2. Requirement Traceability Matrix (Appendix A)
3. User Personas (Section 2.2)
4. MVP Feature Prioritization (Section 4.3)
5. Cross-referenced technical docs (01-07)

**Project Manager Action Items:**
- Create sprint backlog from requirements
- Define user stories with acceptance criteria in project management tool
- Establish velocity baseline (estimate capacity per 2-week sprint)
- Schedule technical design review with Tech Lead
- Create risk register and mitigation tracking

---

**End of Business Requirements Document - Part 1**

**Document Status:** Ready for Review
**Next Document:** Business Requirements Document - Part 2 (Non-Functional Requirements, Data Models, API Specs)
