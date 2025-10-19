# Technical Specifications - Part 1

## Document Purpose

This document provides detailed technical specifications for the Vendo multi-tenant commerce platform. It translates architectural decisions into concrete implementation guidelines with specific configurations, code patterns, and operational parameters.

**Target Audience**: Solo developer, AI agents, future contributors
**Scope**: Technology decisions, distributed systems architecture, observability, multi-tenancy, and API gateway
**Related Documents**: 04_TECH_DECISIONS.md, 03_DEVELOPMENT_ROADMAP.md, 07_SECURITY_AND_COMPLIANCE.md

---

## Table of Contents

1. [Technology Decisions with Rationale](#1-technology-decisions-with-rationale)
2. [Distributed System Architecture](#2-distributed-system-architecture)
3. [Observability Specifications](#3-observability-specifications)
4. [Multi-Tenancy Implementation](#4-multi-tenancy-implementation)
5. [API Gateway Architecture](#5-api-gateway-architecture)

---

## 1. Technology Decisions with Rationale

### 1.1 Identity Provider: OpenIddict (Recommended)

**Decision**: Use **OpenIddict** instead of Duende IdentityServer for OAuth2/OpenID Connect implementation.

**Rationale**:
- **Free and Open Source**: No licensing costs (Duende IdentityServer requires commercial license beyond development)
- **ASP.NET Core Native**: First-class integration with ASP.NET Core Identity
- **Active Development**: Well-maintained with regular security updates
- **Flexible**: Supports all standard OAuth2/OIDC flows
- **Solo-Developer Friendly**: Simpler configuration than IdentityServer4/Duende
- **Production Ready**: Used by major organizations and SaaS platforms

**Alternative Considered**: Duende IdentityServer
- **Rejected because**: Requires commercial license ($1,500+/year for production)
- **When to reconsider**: If enterprise features like dynamic client registration, advanced federation, or commercial support become critical

#### 1.1.1 OpenIddict Configuration

**NuGet Packages**:
```xml
<PackageReference Include="OpenIddict.AspNetCore" Version="5.0.0" />
<PackageReference Include="OpenIddict.EntityFrameworkCore" Version="5.0.0" />
```

**Startup Configuration** (`Program.cs`):
```csharp
builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
               .UseDbContext<IdentityDbContext>();
    })
    .AddServer(options =>
    {
        // Enable required flows
        options.SetAuthorizationEndpointUris("/connect/authorize")
               .SetTokenEndpointUris("/connect/token")
               .SetUserinfoEndpointUris("/connect/userinfo")
               .SetLogoutEndpointUris("/connect/logout");

        // Enable required grant types
        options.AllowAuthorizationCodeFlow()
               .RequireProofKeyForCodeExchange() // PKCE for SPAs
               .AllowClientCredentialsFlow()      // Service-to-service
               .AllowRefreshTokenFlow();

        // Token lifetimes
        options.SetAccessTokenLifetime(TimeSpan.FromMinutes(30));
        options.SetRefreshTokenLifetime(TimeSpan.FromDays(14));

        // Development only: use developer signing certificate
        options.AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate();

        // Production: load from configuration/key vault
        // options.AddSigningCertificate(certificate);

        // ASP.NET Core integration
        options.UseAspNetCore()
               .EnableAuthorizationEndpointPassthrough()
               .EnableTokenEndpointPassthrough()
               .EnableUserinfoEndpointPassthrough()
               .EnableLogoutEndpointPassthrough();
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });
```

**Client Registration** (Seed data for Angular SPA):
```csharp
public async Task SeedClientsAsync()
{
    var client = await _clientManager.FindByClientIdAsync("vendo-angular-spa");
    if (client == null)
    {
        await _clientManager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = "vendo-angular-spa",
            DisplayName = "Vendo Angular SPA",
            Type = ClientTypes.Public, // No client secret for SPAs

            RedirectUris =
            {
                new Uri("http://localhost:4200/auth/callback"),
                new Uri("https://*.vendo.app/auth/callback")
            },
            PostLogoutRedirectUris =
            {
                new Uri("http://localhost:4200/"),
                new Uri("https://*.vendo.app/")
            },

            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Token,
                Permissions.Endpoints.Logout,

                Permissions.GrantTypes.AuthorizationCode,
                Permissions.GrantTypes.RefreshToken,

                Permissions.ResponseTypes.Code,

                Permissions.Scopes.Email,
                Permissions.Scopes.Profile,
                Permissions.Scopes.Roles,

                // Custom scopes
                "vendo.products.read",
                "vendo.products.write",
                "vendo.orders.read",
                "vendo.orders.write"
            },

            Requirements = { Requirements.Features.ProofKeyForCodeExchange }
        });
    }
}
```

**Token Claims Configuration**:
```csharp
public class CustomClaimsProvider : IOpenIddictServerHandler<HandleUserinfoRequestContext>
{
    private readonly ITenantContext _tenantContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public async ValueTask HandleAsync(HandleUserinfoRequestContext context)
    {
        var user = await _userManager.GetUserAsync(context.Principal);

        // Add tenant claim
        context.Principal.SetClaim("tenant_id", user.TenantId.ToString());

        // Add role claims
        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            context.Principal.SetClaim("role", role);
        }

        // Add store context if applicable
        if (user.DefaultStoreId.HasValue)
        {
            context.Principal.SetClaim("store_id", user.DefaultStoreId.ToString());
        }
    }
}
```

**Angular Integration** (using `angular-oauth2-oidc`):
```typescript
// auth.config.ts
import { AuthConfig } from 'angular-oauth2-oidc';

export const authConfig: AuthConfig = {
  issuer: 'https://auth.vendo.app',
  clientId: 'vendo-angular-spa',
  responseType: 'code',
  redirectUri: window.location.origin + '/auth/callback',
  silentRefreshRedirectUri: window.location.origin + '/silent-refresh.html',
  scope: 'openid profile email vendo.products.read vendo.products.write vendo.orders.read vendo.orders.write',
  useSilentRefresh: true,
  sessionChecksEnabled: true,
  showDebugInformation: false,
  clearHashAfterLogin: true,
  requireHttps: true, // Set to false for local dev
};
```

---

### 1.2 Payment Provider: Stripe (MVP)

**Decision**: Use **Stripe** as the initial payment provider with Checkout Sessions (hosted checkout).

**Rationale**:
- **Developer Experience**: Best-in-class documentation and SDKs
- **Hosted Checkout**: Minimal PCI scope via Stripe Checkout
- **Global Coverage**: Supports 135+ currencies and 45+ countries
- **Pricing Transparency**: 2.9% + $0.30 per transaction (US), no monthly fees
- **Extensibility**: Easy to add additional payment methods (Apple Pay, Google Pay, Buy Now Pay Later)
- **Webhooks**: Reliable event system for payment status updates
- **Solo-Friendly**: No merchant account setup required

**Alternative Considered**: PayPal, Square, Adyen
- **Rejected for MVP because**: Stripe offers best developer experience for solo developer
- **When to add**: Phase 2 - implement payment provider abstraction to support multiple gateways

#### 1.2.1 Stripe Integration Architecture

**NuGet Package**:
```xml
<PackageReference Include="Stripe.net" Version="43.0.0" />
```

**Configuration** (`appsettings.json`):
```json
{
  "Stripe": {
    "SecretKey": "sk_test_...", // From environment variable in production
    "PublishableKey": "pk_test_...",
    "WebhookSecret": "whsec_...",
    "CheckoutSuccessUrl": "https://{storeDomain}/checkout/success?session_id={CHECKOUT_SESSION_ID}",
    "CheckoutCancelUrl": "https://{storeDomain}/checkout/cancel"
  }
}
```

**Service Abstraction** (`IPaymentProvider.cs`):
```csharp
public interface IPaymentProvider
{
    Task<PaymentSessionResult> CreateCheckoutSessionAsync(
        CreatePaymentSessionRequest request,
        CancellationToken cancellationToken);

    Task<PaymentVerificationResult> VerifyWebhookSignatureAsync(
        string payload,
        string signature);

    Task<PaymentStatus> GetPaymentStatusAsync(
        string providerTransactionId,
        CancellationToken cancellationToken);

    Task<RefundResult> RefundPaymentAsync(
        string providerTransactionId,
        decimal amount,
        CancellationToken cancellationToken);
}

public record CreatePaymentSessionRequest(
    Guid OrderId,
    Guid TenantId,
    decimal Amount,
    string Currency,
    string CustomerEmail,
    Dictionary<string, string> Metadata);

public record PaymentSessionResult(
    bool Success,
    string SessionId,
    string CheckoutUrl,
    string ErrorMessage = null);
```

**Stripe Implementation** (`StripePaymentProvider.cs`):
```csharp
public class StripePaymentProvider : IPaymentProvider
{
    private readonly StripeClient _client;
    private readonly IOptions<StripeSettings> _settings;
    private readonly ILogger<StripePaymentProvider> _logger;

    public async Task<PaymentSessionResult> CreateCheckoutSessionAsync(
        CreatePaymentSessionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new()
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = request.Currency.ToLower(),
                            UnitAmount = (long)(request.Amount * 100), // Convert to cents
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Order #{request.OrderId}",
                            },
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = _settings.Value.CheckoutSuccessUrl,
                CancelUrl = _settings.Value.CheckoutCancelUrl,
                CustomerEmail = request.CustomerEmail,
                Metadata = request.Metadata,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            };

            var service = new SessionService(_client);
            var session = await service.CreateAsync(options, cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Created Stripe checkout session {SessionId} for order {OrderId}",
                session.Id,
                request.OrderId);

            return new PaymentSessionResult(
                Success: true,
                SessionId: session.Id,
                CheckoutUrl: session.Url);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex,
                "Failed to create Stripe checkout session for order {OrderId}",
                request.OrderId);

            return new PaymentSessionResult(
                Success: false,
                SessionId: null,
                CheckoutUrl: null,
                ErrorMessage: ex.Message);
        }
    }

    public async Task<PaymentVerificationResult> VerifyWebhookSignatureAsync(
        string payload,
        string signature)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                payload,
                signature,
                _settings.Value.WebhookSecret,
                throwOnApiVersionMismatch: false);

            return new PaymentVerificationResult(
                IsValid: true,
                Event: stripeEvent);
        }
        catch (StripeException ex)
        {
            _logger.LogWarning(ex, "Invalid Stripe webhook signature");
            return new PaymentVerificationResult(IsValid: false, Event: null);
        }
    }
}
```

**Webhook Controller** (`PaymentWebhookController.cs`):
```csharp
[ApiController]
[Route("api/webhooks/stripe")]
[AllowAnonymous] // Authenticated via signature verification
public class StripeWebhookController : ControllerBase
{
    private readonly IPaymentProvider _paymentProvider;
    private readonly IMediator _mediator;
    private readonly ILogger<StripeWebhookController> _logger;

    [HttpPost]
    [RequestSizeLimit(10_485_760)] // 10MB limit
    public async Task<IActionResult> HandleWebhook()
    {
        var payload = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"].FirstOrDefault();

        if (string.IsNullOrEmpty(signature))
        {
            _logger.LogWarning("Stripe webhook received without signature");
            return BadRequest("Missing signature");
        }

        var verification = await _paymentProvider.VerifyWebhookSignatureAsync(payload, signature);
        if (!verification.IsValid)
        {
            return Unauthorized("Invalid signature");
        }

        var stripeEvent = verification.Event;

        // Idempotency check
        var isDuplicate = await _mediator.Send(
            new CheckWebhookIdempotencyQuery(stripeEvent.Id));
        if (isDuplicate)
        {
            _logger.LogInformation("Duplicate webhook {EventId} ignored", stripeEvent.Id);
            return Ok(); // Return 200 to prevent retries
        }

        try
        {
            switch (stripeEvent.Type)
            {
                case Events.CheckoutSessionCompleted:
                    var session = stripeEvent.Data.Object as Session;
                    await _mediator.Send(new ProcessPaymentSuccessCommand(
                        ProviderTransactionId: session.Id,
                        OrderId: Guid.Parse(session.Metadata["OrderId"]),
                        TenantId: Guid.Parse(session.Metadata["TenantId"]),
                        Amount: session.AmountTotal.Value / 100m,
                        Currency: session.Currency.ToUpper()));
                    break;

                case Events.CheckoutSessionExpired:
                    var expiredSession = stripeEvent.Data.Object as Session;
                    await _mediator.Send(new ProcessPaymentExpiredCommand(
                        ProviderTransactionId: expiredSession.Id,
                        OrderId: Guid.Parse(expiredSession.Metadata["OrderId"])));
                    break;

                case Events.ChargeRefunded:
                    var refund = stripeEvent.Data.Object as Refund;
                    await _mediator.Send(new ProcessRefundCommand(
                        ProviderTransactionId: refund.Id,
                        Amount: refund.Amount / 100m));
                    break;

                default:
                    _logger.LogInformation("Unhandled Stripe event type: {EventType}", stripeEvent.Type);
                    break;
            }

            // Mark webhook as processed
            await _mediator.Send(new RecordWebhookEventCommand(
                EventId: stripeEvent.Id,
                EventType: stripeEvent.Type,
                ProcessedAt: DateTime.UtcNow));

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Stripe webhook {EventId}", stripeEvent.Id);
            return StatusCode(500); // Stripe will retry
        }
    }
}
```

**Frontend Checkout Flow** (Angular):
```typescript
// checkout.service.ts
export class CheckoutService {
  async initiateCheckout(orderId: string): Promise<void> {
    const response = await this.http.post<CheckoutSessionResponse>(
      `/api/v1/orders/${orderId}/checkout`,
      {}
    ).toPromise();

    if (response.success) {
      // Redirect to Stripe Checkout
      window.location.href = response.checkoutUrl;
    } else {
      this.handleCheckoutError(response.errorMessage);
    }
  }
}
```

---

### 1.3 Monitoring Stack: Application Insights (MVP) → Prometheus + Grafana (Scale)

**Decision**: Start with **Azure Application Insights** for MVP, migrate to **Prometheus + Grafana** in Phase 3.

#### 1.3.1 MVP: Application Insights

**Rationale**:
- **Turnkey Solution**: Minimal configuration required
- **Integrated**: Works seamlessly with ASP.NET Core and Azure hosting
- **Cost-Effective for Small Scale**: Free tier includes 5GB/month
- **Developer-Friendly**: Rich querying with KQL, automatic dependency tracking
- **Cloud-Agnostic Enough**: Can run on any hosting with SDK

**NuGet Package**:
```xml
<PackageReference Include="Microsoft.ApplicationInsights.AspNetCore" Version="2.21.0" />
```

**Configuration** (`Program.cs`):
```csharp
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
    options.EnableAdaptiveSampling = true; // Reduce costs
    options.EnableQuickPulseMetricStream = true; // Live metrics
});

// Custom telemetry enrichment
builder.Services.AddSingleton<ITelemetryInitializer, TenantTelemetryInitializer>();
```

**Custom Telemetry Initializer**:
```csharp
public class TenantTelemetryInitializer : ITelemetryInitializer
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public void Initialize(ITelemetry telemetry)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return;

        // Add tenant context to all telemetry
        if (telemetry is ISupportProperties propertyTelemetry)
        {
            var tenantId = context.User?.FindFirst("tenant_id")?.Value;
            if (!string.IsNullOrEmpty(tenantId))
            {
                propertyTelemetry.Properties["TenantId"] = tenantId;
            }

            var correlationId = context.TraceIdentifier;
            propertyTelemetry.Properties["CorrelationId"] = correlationId;
        }
    }
}
```

**Custom Metrics Tracking**:
```csharp
public class OrderMetricsService
{
    private readonly TelemetryClient _telemetryClient;

    public void TrackOrderCreated(Order order)
    {
        _telemetryClient.TrackEvent("OrderCreated", new Dictionary<string, string>
        {
            ["TenantId"] = order.TenantId.ToString(),
            ["OrderId"] = order.Id.ToString(),
            ["Status"] = order.Status.ToString()
        }, new Dictionary<string, double>
        {
            ["Amount"] = (double)order.TotalAmount,
            ["ItemCount"] = order.Items.Count
        });

        _telemetryClient.GetMetric("Orders.Created", "TenantId")
            .TrackValue(1, order.TenantId.ToString());
    }

    public void TrackPaymentProcessed(Payment payment)
    {
        _telemetryClient.GetMetric("Payments.Amount", "TenantId", "Currency")
            .TrackValue((double)payment.Amount, payment.TenantId.ToString(), payment.Currency);
    }
}
```

**Sample KQL Queries**:
```kql
// Average API response time per endpoint
requests
| where timestamp > ago(1h)
| summarize avg(duration), percentile(duration, 95) by name
| order by avg_duration desc

// Error rate per tenant
exceptions
| where timestamp > ago(1h)
| extend TenantId = tostring(customDimensions.TenantId)
| summarize ErrorCount = count() by TenantId
| order by ErrorCount desc

// Payment success rate
customEvents
| where name in ("PaymentSuccess", "PaymentFailed")
| where timestamp > ago(24h)
| summarize Total = count() by name
| extend SuccessRate = round(100.0 * todouble(Total) / todouble(toscalar(customEvents | where name in ("PaymentSuccess", "PaymentFailed") | count())), 2)
```

#### 1.3.2 Phase 3: Prometheus + Grafana

**Rationale for Migration**:
- **Open Source**: No vendor lock-in, no per-GB costs
- **Flexibility**: Custom exporters, unlimited retention policies
- **Alerting**: Powerful AlertManager integration
- **Community**: Rich ecosystem of dashboards and exporters

**NuGet Package**:
```xml
<PackageReference Include="prometheus-net.AspNetCore" Version="8.0.1" />
```

**Configuration** (`Program.cs`):
```csharp
builder.Services.AddSingleton<IMetricsRegistry, MetricsRegistry>();

var app = builder.Build();

// Expose /metrics endpoint
app.UseMetricServer(); // Defaults to /metrics
app.UseHttpMetrics(); // Automatic HTTP metrics
```

**Custom Metrics**:
```csharp
public class MetricsRegistry : IMetricsRegistry
{
    // Counters
    public Counter OrdersCreated { get; } = Metrics.CreateCounter(
        "vendo_orders_created_total",
        "Total number of orders created",
        new CounterConfiguration { LabelNames = new[] { "tenant_id", "status" } });

    public Counter PaymentsProcessed { get; } = Metrics.CreateCounter(
        "vendo_payments_processed_total",
        "Total number of payments processed",
        new CounterConfiguration { LabelNames = new[] { "tenant_id", "status", "provider" } });

    // Gauges
    public Gauge ActiveOrders { get; } = Metrics.CreateGauge(
        "vendo_active_orders",
        "Number of active orders",
        new GaugeConfiguration { LabelNames = new[] { "tenant_id" } });

    // Histograms (for request duration)
    public Histogram OrderProcessingDuration { get; } = Metrics.CreateHistogram(
        "vendo_order_processing_duration_seconds",
        "Order processing duration in seconds",
        new HistogramConfiguration
        {
            LabelNames = new[] { "tenant_id", "operation" },
            Buckets = Histogram.ExponentialBuckets(0.001, 2, 10) // 1ms to ~1s
        });

    // Summary (for percentiles)
    public Summary ApiRequestDuration { get; } = Metrics.CreateSummary(
        "vendo_api_request_duration_seconds",
        "API request duration in seconds",
        new SummaryConfiguration
        {
            LabelNames = new[] { "endpoint", "method" },
            Objectives = new[]
            {
                new QuantileEpsilonPair(0.5, 0.05),  // Median
                new QuantileEpsilonPair(0.95, 0.01), // 95th percentile
                new QuantileEpsilonPair(0.99, 0.001) // 99th percentile
            }
        });
}
```

**Grafana Dashboard JSON** (sample):
```json
{
  "dashboard": {
    "title": "Vendo Platform Overview",
    "panels": [
      {
        "title": "Request Rate (req/s)",
        "targets": [
          {
            "expr": "rate(http_requests_received_total[5m])"
          }
        ]
      },
      {
        "title": "Error Rate (%)",
        "targets": [
          {
            "expr": "100 * rate(http_requests_received_total{code=~\"5..\"}[5m]) / rate(http_requests_received_total[5m])"
          }
        ]
      },
      {
        "title": "P95 Response Time",
        "targets": [
          {
            "expr": "histogram_quantile(0.95, rate(vendo_api_request_duration_seconds_bucket[5m]))"
          }
        ]
      }
    ]
  }
}
```

---

## 2. Distributed System Architecture

### 2.1 Service Boundaries

The platform is decomposed into the following microservices:

| Service | Responsibility | Database | Port (Dev) |
|---------|---------------|----------|------------|
| **Auth Service** | Authentication, authorization, user management | `VendoAuth` | 5001 |
| **Tenant Service** | Store registration, configuration, subscription | `VendoTenant` | 5002 |
| **Catalog Service** | Products, categories, inventory management | `VendoCatalog` | 5003 |
| **Order Service** | Order lifecycle, cart, checkout coordination | `VendoOrder` | 5004 |
| **Payment Service** | Payment provider integration, webhooks | `VendoPayment` | 5005 |
| **Notification Service** | Email, SMS, push notifications | `VendoNotification` | 5006 |
| **Analytics Service** (Phase 3) | Reporting, dashboards, metrics | `VendoAnalytics` | 5007 |

**Service Communication Matrix**:

| From Service | To Service | Pattern | Protocol | Purpose |
|-------------|-----------|---------|----------|---------|
| Order | Catalog | Sync | REST | Check product availability |
| Order | Payment | Async | RabbitMQ | Create payment session |
| Payment | Order | Async | RabbitMQ | Confirm payment success |
| Order | Notification | Async | RabbitMQ | Send order confirmation |
| All | Auth | Sync | REST | Token validation |

---

### 2.2 Resilience Patterns with Polly

**Decision**: Use **Polly** for resilience patterns across all HTTP and message-based communication.

**NuGet Package**:
```xml
<PackageReference Include="Polly" Version="8.0.0" />
<PackageReference Include="Polly.Extensions.Http" Version="3.0.0" />
<PackageReference Include="Microsoft.Extensions.Http.Polly" Version="8.0.0" />
```

#### 2.2.1 Circuit Breaker Pattern

**Purpose**: Prevent cascading failures by stopping calls to failing services.

**Configuration**:
```csharp
public static class ResiliencePolicies
{
    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(
        ILogger logger,
        string serviceName)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, timespan) =>
                {
                    logger.LogWarning(
                        "Circuit breaker opened for {ServiceName}. Breaking for {Duration}s",
                        serviceName,
                        timespan.TotalSeconds);
                },
                onReset: () =>
                {
                    logger.LogInformation(
                        "Circuit breaker reset for {ServiceName}",
                        serviceName);
                },
                onHalfOpen: () =>
                {
                    logger.LogInformation(
                        "Circuit breaker half-open for {ServiceName}",
                        serviceName);
                });
    }
}
```

#### 2.2.2 Retry Pattern with Exponential Backoff

**Purpose**: Retry transient failures with increasing delays.

**Configuration**:
```csharp
public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(
    ILogger logger,
    string serviceName)
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // 2s, 4s, 8s
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                logger.LogWarning(
                    "Retry {RetryCount} for {ServiceName} after {Delay}s. Reason: {Reason}",
                    retryCount,
                    serviceName,
                    timespan.TotalSeconds,
                    outcome.Exception?.Message ?? outcome.Result.ReasonPhrase);
            });
}
```

#### 2.2.3 Timeout Pattern

**Purpose**: Prevent indefinite waits for slow services.

**Configuration**:
```csharp
public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(
    ILogger logger,
    string serviceName,
    int timeoutSeconds = 10)
{
    return Policy
        .TimeoutAsync<HttpResponseMessage>(
            timeout: TimeSpan.FromSeconds(timeoutSeconds),
            onTimeoutAsync: (context, timespan, task) =>
            {
                logger.LogWarning(
                    "Request to {ServiceName} timed out after {Timeout}s",
                    serviceName,
                    timespan.TotalSeconds);
                return Task.CompletedTask;
            });
}
```

#### 2.2.4 Combining Policies (Wrap Pattern)

**Configuration** (`Program.cs`):
```csharp
builder.Services.AddHttpClient<ICatalogServiceClient, CatalogServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:Catalog:Url"]);
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddPolicyHandler((services, request) =>
{
    var logger = services.GetRequiredService<ILogger<CatalogServiceClient>>();

    // Wrap policies: Retry -> Circuit Breaker -> Timeout
    return Policy.WrapAsync(
        ResiliencePolicies.GetRetryPolicy(logger, "CatalogService"),
        ResiliencePolicies.GetCircuitBreakerPolicy(logger, "CatalogService"),
        ResiliencePolicies.GetTimeoutPolicy(logger, "CatalogService", timeoutSeconds: 5));
});
```

**Usage in Service Client**:
```csharp
public class CatalogServiceClient : ICatalogServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CatalogServiceClient> _logger;

    public async Task<ProductAvailabilityResponse> CheckAvailabilityAsync(
        Guid productId,
        int quantity,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/api/v1/products/{productId}/availability?quantity={quantity}",
                cancellationToken);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<ProductAvailabilityResponse>(
                cancellationToken: cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex,
                "Failed to check product availability for {ProductId}",
                productId);
            throw new ServiceCommunicationException("Catalog service unavailable", ex);
        }
    }
}
```

---

### 2.3 Service Communication

#### 2.3.1 Synchronous: REST

**Use Cases**:
- Real-time data queries (product details, availability)
- User-facing operations requiring immediate response
- Token validation

**HTTP Client Best Practices**:
```csharp
// ✅ DO: Use IHttpClientFactory
services.AddHttpClient<IProductServiceClient, ProductServiceClient>();

// ❌ DON'T: Create HttpClient instances directly
// var client = new HttpClient(); // Socket exhaustion!

// ✅ DO: Include correlation ID in all requests
public class CorrelationIdDelegatingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var correlationId = _httpContextAccessor.HttpContext?.TraceIdentifier
            ?? Guid.NewGuid().ToString();

        request.Headers.Add("X-Correlation-ID", correlationId);
        request.Headers.Add("X-Tenant-ID", GetCurrentTenantId());

        return await base.SendAsync(request, cancellationToken);
    }
}

// Register handler
services.AddTransient<CorrelationIdDelegatingHandler>();
services.AddHttpClient<IOrderServiceClient, OrderServiceClient>()
    .AddHttpMessageHandler<CorrelationIdDelegatingHandler>();
```

#### 2.3.2 Asynchronous: RabbitMQ

**Use Cases**:
- Order placement → Payment processing
- Payment confirmation → Order fulfillment
- Order confirmation → Email notification
- Inventory updates → Analytics

**NuGet Package**:
```xml
<PackageReference Include="MassTransit.RabbitMQ" Version="8.1.0" />
```

**Configuration** (`Program.cs`):
```csharp
builder.Services.AddMassTransit(x =>
{
    // Add consumers
    x.AddConsumer<OrderCreatedConsumer>();
    x.AddConsumer<PaymentSuccessConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]);
            h.Password(builder.Configuration["RabbitMQ:Password"]);
        });

        // Configure exchanges and queues
        cfg.Message<OrderCreatedEvent>(x => x.SetEntityName("order-events"));
        cfg.Message<PaymentSuccessEvent>(x => x.SetEntityName("payment-events"));

        // Configure retry and error handling
        cfg.UseMessageRetry(r => r.Exponential(
            retryLimit: 5,
            minInterval: TimeSpan.FromSeconds(2),
            maxInterval: TimeSpan.FromMinutes(5),
            intervalDelta: TimeSpan.FromSeconds(2)));

        cfg.ConfigureEndpoints(context);
    });
});
```

**Event Contracts** (`Shared.Events` project):
```csharp
public record OrderCreatedEvent
{
    public Guid OrderId { get; init; }
    public Guid TenantId { get; init; }
    public Guid CustomerId { get; init; }
    public decimal TotalAmount { get; init; }
    public string Currency { get; init; }
    public DateTime CreatedAt { get; init; }
    public string CorrelationId { get; init; }
}

public record PaymentSuccessEvent
{
    public Guid PaymentId { get; init; }
    public Guid OrderId { get; init; }
    public Guid TenantId { get; init; }
    public string ProviderTransactionId { get; init; }
    public decimal Amount { get; init; }
    public DateTime ProcessedAt { get; init; }
    public string CorrelationId { get; init; }
}
```

**Publishing Events**:
```csharp
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderResult>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IOrderRepository _orderRepository;

    public async Task<OrderResult> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        var order = Order.Create(/* ... */);
        await _orderRepository.AddAsync(order, cancellationToken);

        // Publish event after transaction commits
        await _publishEndpoint.Publish(new OrderCreatedEvent
        {
            OrderId = order.Id,
            TenantId = order.TenantId,
            CustomerId = order.CustomerId,
            TotalAmount = order.TotalAmount,
            Currency = order.Currency,
            CreatedAt = order.CreatedAt,
            CorrelationId = Activity.Current?.Id ?? Guid.NewGuid().ToString()
        }, cancellationToken);

        return new OrderResult(order.Id);
    }
}
```

**Consuming Events**:
```csharp
public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly IPaymentProvider _paymentProvider;
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var evt = context.Message;

        _logger.LogInformation(
            "Processing order created event for order {OrderId}, tenant {TenantId}",
            evt.OrderId,
            evt.TenantId);

        try
        {
            var result = await _paymentProvider.CreateCheckoutSessionAsync(
                new CreatePaymentSessionRequest(
                    OrderId: evt.OrderId,
                    TenantId: evt.TenantId,
                    Amount: evt.TotalAmount,
                    Currency: evt.Currency,
                    CustomerEmail: await GetCustomerEmail(evt.CustomerId),
                    Metadata: new Dictionary<string, string>
                    {
                        ["OrderId"] = evt.OrderId.ToString(),
                        ["TenantId"] = evt.TenantId.ToString(),
                        ["CorrelationId"] = evt.CorrelationId
                    }),
                CancellationToken.None);

            if (result.Success)
            {
                await context.Publish(new PaymentSessionCreatedEvent
                {
                    OrderId = evt.OrderId,
                    CheckoutUrl = result.CheckoutUrl,
                    SessionId = result.SessionId
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to process order created event for {OrderId}",
                evt.OrderId);
            throw; // MassTransit will retry
        }
    }
}
```

---

### 2.4 Distributed Tracing with OpenTelemetry

**Decision**: Use **OpenTelemetry** for distributed tracing across all services.

**NuGet Packages**:
```xml
<PackageReference Include="OpenTelemetry.Exporter.Console" Version="1.6.0" />
<PackageReference Include="OpenTelemetry.Exporter.Jaeger" Version="1.6.0" />
<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.6.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.5.1" />
<PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.5.1" />
<PackageReference Include="OpenTelemetry.Instrumentation.SqlClient" Version="1.5.1" />
```

**Configuration** (`Program.cs`):
```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .AddSource("Vendo.*") // Capture custom activities
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService(
                    serviceName: "Vendo.OrderService",
                    serviceVersion: "1.0.0"))
            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true;
                options.EnrichWithHttpRequest = (activity, request) =>
                {
                    activity.SetTag("tenant_id", request.HttpContext.User?.FindFirst("tenant_id")?.Value);
                    activity.SetTag("user_id", request.HttpContext.User?.FindFirst("sub")?.Value);
                };
            })
            .AddHttpClientInstrumentation(options =>
            {
                options.RecordException = true;
                options.EnrichWithHttpRequestMessage = (activity, request) =>
                {
                    activity.SetTag("http.request.correlation_id",
                        request.Headers.GetValues("X-Correlation-ID").FirstOrDefault());
                };
            })
            .AddSqlClientInstrumentation(options =>
            {
                options.SetDbStatementForText = true;
                options.RecordException = true;
            })
            .AddJaegerExporter(options =>
            {
                options.AgentHost = builder.Configuration["Jaeger:Host"];
                options.AgentPort = int.Parse(builder.Configuration["Jaeger:Port"]);
            });
    });
```

**Custom Activity/Span Creation**:
```csharp
public class OrderService
{
    private static readonly ActivitySource ActivitySource = new("Vendo.OrderService");

    public async Task<Order> ProcessOrderAsync(CreateOrderRequest request)
    {
        using var activity = ActivitySource.StartActivity("ProcessOrder", ActivityKind.Internal);

        activity?.SetTag("tenant_id", request.TenantId);
        activity?.SetTag("order.items_count", request.Items.Count);
        activity?.SetTag("order.total_amount", request.TotalAmount);

        try
        {
            var order = await CreateOrderAsync(request);

            activity?.SetTag("order.id", order.Id);
            activity?.SetStatus(ActivityStatusCode.Ok);

            return order;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            throw;
        }
    }

    private async Task<Order> CreateOrderAsync(CreateOrderRequest request)
    {
        using var activity = ActivitySource.StartActivity("CreateOrder", ActivityKind.Internal);

        // Validate inventory
        using (var validateActivity = ActivitySource.StartActivity("ValidateInventory"))
        {
            await ValidateInventoryAsync(request.Items);
        }

        // Calculate totals
        using (var calculateActivity = ActivitySource.StartActivity("CalculateTotals"))
        {
            var totals = CalculateTotals(request.Items);
            calculateActivity?.SetTag("order.subtotal", totals.Subtotal);
            calculateActivity?.SetTag("order.tax", totals.Tax);
        }

        // Save order
        using (var saveActivity = ActivitySource.StartActivity("SaveOrder"))
        {
            return await _repository.SaveAsync(order);
        }
    }
}
```

**Correlation ID Propagation**:
```csharp
// Middleware to ensure correlation ID
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeaderName = "X-Correlation-ID";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeaderName].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        context.TraceIdentifier = correlationId;
        context.Response.Headers[CorrelationIdHeaderName] = correlationId;

        // Set on current activity
        Activity.Current?.SetTag("correlation_id", correlationId);

        await _next(context);
    }
}

// Register middleware
app.UseMiddleware<CorrelationIdMiddleware>();
```

---

### 2.5 Saga Pattern for Order + Payment Coordination

**Decision**: Use **MassTransit Saga State Machine** for orchestrating long-running order-payment workflows.

**Rationale**:
- **Consistency**: Ensures order and payment states remain synchronized
- **Resilience**: Handles failures and compensating transactions
- **Observability**: Clear state transitions for debugging
- **Timeout Handling**: Automatic cleanup of abandoned checkouts

**Saga State Definition**:
```csharp
public class OrderSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; } // OrderId
    public string CurrentState { get; set; }

    // Order data
    public Guid TenantId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; }

    // Payment data
    public string CheckoutSessionId { get; set; }
    public string ProviderTransactionId { get; set; }

    // Timing
    public DateTime CreatedAt { get; set; }
    public DateTime? PaymentInitiatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Timeout tracking
    public Guid? PaymentTimeoutTokenId { get; set; }
}
```

**Saga State Machine**:
```csharp
public class OrderSaga : MassTransitStateMachine<OrderSagaState>
{
    public OrderSaga()
    {
        InstanceState(x => x.CurrentState);

        Event(() => OrderCreated, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => PaymentSessionCreated, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => PaymentSuccess, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => PaymentFailed, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => PaymentTimeout, x => x.CorrelateById(m => m.Message.OrderId));

        Initially(
            When(OrderCreated)
                .Then(context =>
                {
                    context.Saga.TenantId = context.Message.TenantId;
                    context.Saga.CustomerId = context.Message.CustomerId;
                    context.Saga.TotalAmount = context.Message.TotalAmount;
                    context.Saga.Currency = context.Message.Currency;
                    context.Saga.CreatedAt = context.Message.CreatedAt;
                })
                .TransitionTo(AwaitingPaymentSession)
                .Publish(context => new InitiatePaymentCommand
                {
                    OrderId = context.Saga.CorrelationId,
                    TenantId = context.Saga.TenantId,
                    Amount = context.Saga.TotalAmount,
                    Currency = context.Saga.Currency
                }));

        During(AwaitingPaymentSession,
            When(PaymentSessionCreated)
                .Then(context =>
                {
                    context.Saga.CheckoutSessionId = context.Message.SessionId;
                    context.Saga.PaymentInitiatedAt = DateTime.UtcNow;
                })
                .Schedule(PaymentTimeout, context => context.Init<PaymentTimeoutEvent>(new
                {
                    OrderId = context.Saga.CorrelationId
                }), context => TimeSpan.FromMinutes(30))
                .TransitionTo(AwaitingPayment));

        During(AwaitingPayment,
            When(PaymentSuccess)
                .Unschedule(PaymentTimeout)
                .Then(context =>
                {
                    context.Saga.ProviderTransactionId = context.Message.ProviderTransactionId;
                    context.Saga.CompletedAt = DateTime.UtcNow;
                })
                .Publish(context => new OrderConfirmedEvent
                {
                    OrderId = context.Saga.CorrelationId,
                    TenantId = context.Saga.TenantId,
                    CustomerId = context.Saga.CustomerId,
                    ConfirmedAt = DateTime.UtcNow
                })
                .TransitionTo(Completed)
                .Finalize(),

            When(PaymentFailed)
                .Unschedule(PaymentTimeout)
                .Publish(context => new OrderCancelledEvent
                {
                    OrderId = context.Saga.CorrelationId,
                    Reason = "Payment failed",
                    CancelledAt = DateTime.UtcNow
                })
                .TransitionTo(Cancelled)
                .Finalize(),

            When(PaymentTimeout)
                .Publish(context => new OrderCancelledEvent
                {
                    OrderId = context.Saga.CorrelationId,
                    Reason = "Payment timeout",
                    CancelledAt = DateTime.UtcNow
                })
                .TransitionTo(Cancelled)
                .Finalize());

        SetCompletedWhenFinalized();
    }

    public State AwaitingPaymentSession { get; private set; }
    public State AwaitingPayment { get; private set; }
    public State Completed { get; private set; }
    public State Cancelled { get; private set; }

    public Event<OrderCreatedEvent> OrderCreated { get; private set; }
    public Event<PaymentSessionCreatedEvent> PaymentSessionCreated { get; private set; }
    public Event<PaymentSuccessEvent> PaymentSuccess { get; private set; }
    public Event<PaymentFailedEvent> PaymentFailed { get; private set; }

    public Schedule<OrderSagaState, PaymentTimeoutEvent> PaymentTimeout { get; private set; }
}
```

**Saga Registration**:
```csharp
builder.Services.AddMassTransit(x =>
{
    x.AddSagaStateMachine<OrderSaga, OrderSagaState>()
        .EntityFrameworkRepository(r =>
        {
            r.ConcurrencyMode = ConcurrencyMode.Pessimistic;
            r.AddDbContext<DbContext, OrderSagaDbContext>((provider, builder) =>
            {
                builder.UseSqlServer(
                    provider.GetRequiredService<IConfiguration>()
                        .GetConnectionString("OrderDb"));
            });
        });

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.UseMessageScheduler(new Uri("queue:scheduler")); // For timeout scheduling
        cfg.ConfigureEndpoints(context);
    });
});
```

**Compensating Transactions** (for inventory rollback on payment failure):
```csharp
public class OrderCancelledConsumer : IConsumer<OrderCancelledEvent>
{
    private readonly ICatalogServiceClient _catalogService;

    public async Task Consume(ConsumeContext<OrderCancelledEvent> context)
    {
        var order = await _orderRepository.GetByIdAsync(context.Message.OrderId);

        // Restore inventory
        foreach (var item in order.Items)
        {
            await _catalogService.RestoreInventoryAsync(
                item.ProductId,
                item.Quantity);
        }

        // Update order status
        order.MarkAsCancelled(context.Message.Reason);
        await _orderRepository.UpdateAsync(order);
    }
}
```

---

## 3. Observability Specifications

### 3.1 RED Metrics (Rate, Errors, Duration)

**Principle**: Every service must expose RED metrics for all endpoints.

#### 3.1.1 Rate (Requests per second)

**Prometheus Metric**:
```csharp
public class RequestRateMetrics
{
    public Counter TotalRequests { get; } = Metrics.CreateCounter(
        "vendo_http_requests_total",
        "Total HTTP requests",
        new CounterConfiguration
        {
            LabelNames = new[] { "service", "endpoint", "method", "status_code", "tenant_id" }
        });
}

// Middleware to track requests
public class MetricsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RequestRateMetrics _metrics;

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();

            _metrics.TotalRequests.WithLabels(
                service: "order-service",
                endpoint: context.Request.Path,
                method: context.Request.Method,
                status_code: context.Response.StatusCode.ToString(),
                tenant_id: context.User?.FindFirst("tenant_id")?.Value ?? "anonymous"
            ).Inc();
        }
    }
}
```

**PromQL Query**:
```promql
# Requests per second (5-minute rate)
rate(vendo_http_requests_total[5m])

# Requests per second by tenant
sum(rate(vendo_http_requests_total[5m])) by (tenant_id)

# Top 10 endpoints by request rate
topk(10, sum(rate(vendo_http_requests_total[5m])) by (endpoint))
```

#### 3.1.2 Errors (Error rate %)

**Prometheus Metric**:
```csharp
public class ErrorRateMetrics
{
    public Counter ErrorRequests { get; } = Metrics.CreateCounter(
        "vendo_http_errors_total",
        "Total HTTP errors (4xx, 5xx)",
        new CounterConfiguration
        {
            LabelNames = new[] { "service", "endpoint", "status_code", "error_type", "tenant_id" }
        });
}

// Exception handling middleware
public class ErrorMetricsMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);

            if (context.Response.StatusCode >= 400)
            {
                _metrics.ErrorRequests.WithLabels(
                    service: "order-service",
                    endpoint: context.Request.Path,
                    status_code: context.Response.StatusCode.ToString(),
                    error_type: GetErrorType(context.Response.StatusCode),
                    tenant_id: GetTenantId(context)
                ).Inc();
            }
        }
        catch (Exception ex)
        {
            _metrics.ErrorRequests.WithLabels(
                service: "order-service",
                endpoint: context.Request.Path,
                status_code: "500",
                error_type: ex.GetType().Name,
                tenant_id: GetTenantId(context)
            ).Inc();

            throw;
        }
    }
}
```

**PromQL Query**:
```promql
# Error rate % (last 5 minutes)
100 * (
  sum(rate(vendo_http_errors_total[5m]))
  /
  sum(rate(vendo_http_requests_total[5m]))
)

# Error rate by endpoint
100 * (
  sum(rate(vendo_http_errors_total[5m])) by (endpoint)
  /
  sum(rate(vendo_http_requests_total[5m])) by (endpoint)
)

# 5xx errors only
rate(vendo_http_errors_total{status_code=~"5.."}[5m])
```

#### 3.1.3 Duration (Response time)

**Prometheus Metric**:
```csharp
public class DurationMetrics
{
    public Histogram RequestDuration { get; } = Metrics.CreateHistogram(
        "vendo_http_request_duration_seconds",
        "HTTP request duration in seconds",
        new HistogramConfiguration
        {
            LabelNames = new[] { "service", "endpoint", "method", "tenant_id" },
            Buckets = new[] { 0.01, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10 }
        });
}

// Tracking middleware
public async Task InvokeAsync(HttpContext context)
{
    var sw = Stopwatch.StartNew();

    try
    {
        await _next(context);
    }
    finally
    {
        sw.Stop();

        _metrics.RequestDuration.WithLabels(
            service: "order-service",
            endpoint: context.Request.Path,
            method: context.Request.Method,
            tenant_id: GetTenantId(context)
        ).Observe(sw.Elapsed.TotalSeconds);
    }
}
```

**PromQL Query**:
```promql
# P50 (median) response time
histogram_quantile(0.5,
  rate(vendo_http_request_duration_seconds_bucket[5m]))

# P95 response time
histogram_quantile(0.95,
  rate(vendo_http_request_duration_seconds_bucket[5m]))

# P99 response time
histogram_quantile(0.99,
  rate(vendo_http_request_duration_seconds_bucket[5m]))

# Average response time by endpoint
avg(rate(vendo_http_request_duration_seconds_sum[5m])) by (endpoint)
```

---

### 3.2 Structured Logging with Correlation IDs

**Decision**: Use **Serilog** with JSON formatting and enrichment.

**NuGet Packages**:
```xml
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="5.0.0" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
<PackageReference Include="Serilog.Enrichers.Environment" Version="2.3.0" />
<PackageReference Include="Serilog.Enrichers.Thread" Version="3.1.0" />
<PackageReference Include="Serilog.Settings.Configuration" Version="8.0.0" />
```

**Configuration** (`Program.cs`):
```csharp
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("Service", "Vendo.OrderService")
    .Enrich.With<TenantEnricher>()
    .Enrich.With<CorrelationIdEnricher>()
    .WriteTo.Console(new JsonFormatter())
    .WriteTo.File(
        formatter: new JsonFormatter(),
        path: "logs/order-service-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .CreateLogger();

builder.Host.UseSerilog();
```

**Custom Enrichers**:
```csharp
public class TenantEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var tenantId = _httpContextAccessor.HttpContext?.User?.FindFirst("tenant_id")?.Value;
        if (!string.IsNullOrEmpty(tenantId))
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TenantId", tenantId));
        }
    }
}

public class CorrelationIdEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var correlationId = _httpContextAccessor.HttpContext?.TraceIdentifier;
        if (!string.IsNullOrEmpty(correlationId))
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("CorrelationId", correlationId));
        }

        // Also include trace/span IDs from OpenTelemetry
        var activity = Activity.Current;
        if (activity != null)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TraceId", activity.TraceId.ToString()));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("SpanId", activity.SpanId.ToString()));
        }
    }
}
```

**Structured Logging Example**:
```csharp
public class OrderService
{
    private readonly ILogger<OrderService> _logger;

    public async Task<Order> CreateOrderAsync(CreateOrderRequest request)
    {
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["TenantId"] = request.TenantId,
            ["CustomerId"] = request.CustomerId,
            ["CorrelationId"] = Activity.Current?.Id
        }))
        {
            _logger.LogInformation(
                "Creating order for tenant {TenantId} with {ItemCount} items, total {TotalAmount} {Currency}",
                request.TenantId,
                request.Items.Count,
                request.TotalAmount,
                request.Currency);

            try
            {
                var order = await _repository.CreateAsync(request);

                _logger.LogInformation(
                    "Order {OrderId} created successfully",
                    order.Id);

                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to create order for tenant {TenantId}",
                    request.TenantId);
                throw;
            }
        }
    }
}
```

**JSON Log Output**:
```json
{
  "Timestamp": "2025-01-15T10:30:45.1234567Z",
  "Level": "Information",
  "MessageTemplate": "Creating order for tenant {TenantId} with {ItemCount} items, total {TotalAmount} {Currency}",
  "Properties": {
    "TenantId": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
    "CustomerId": "f1e2d3c4-b5a6-7890-1234-567890fedcba",
    "ItemCount": 3,
    "TotalAmount": 149.97,
    "Currency": "USD",
    "CorrelationId": "0HN7QKQJQJQ2T:00000001",
    "TraceId": "4bf92f3577b34da6a3ce929d0e0e4736",
    "SpanId": "00f067aa0ba902b7",
    "MachineName": "VENDO-ORDER-SVC-01",
    "EnvironmentName": "Production",
    "Service": "Vendo.OrderService",
    "ThreadId": 42
  }
}
```

---

### 3.3 Alerting Rules and Thresholds

**Decision**: Define alerts using **Prometheus AlertManager** (or Application Insights Alerts for MVP).

#### 3.3.1 Critical Alerts

**High Error Rate**:
```yaml
groups:
  - name: vendo_critical_alerts
    interval: 30s
    rules:
      - alert: HighErrorRate
        expr: |
          (
            sum(rate(vendo_http_errors_total{status_code=~"5.."}[5m]))
            /
            sum(rate(vendo_http_requests_total[5m]))
          ) * 100 > 5
        for: 2m
        labels:
          severity: critical
          team: platform
        annotations:
          summary: "High error rate detected"
          description: "Service {{ $labels.service }} has {{ $value }}% 5xx error rate (threshold: 5%)"
```

**Service Down**:
```yaml
      - alert: ServiceDown
        expr: up{job="vendo-order-service"} == 0
        for: 1m
        labels:
          severity: critical
          team: platform
        annotations:
          summary: "Service is down"
          description: "{{ $labels.job }} has been down for more than 1 minute"
```

**Payment Webhook Failures**:
```yaml
      - alert: PaymentWebhookFailures
        expr: |
          sum(rate(vendo_payment_webhook_errors_total[5m])) > 0.1
        for: 5m
        labels:
          severity: critical
          team: payments
        annotations:
          summary: "Payment webhook failures detected"
          description: "{{ $value }} payment webhooks failed in the last 5 minutes"
```

#### 3.3.2 Warning Alerts

**High Response Time**:
```yaml
  - name: vendo_warning_alerts
    interval: 1m
    rules:
      - alert: HighResponseTime
        expr: |
          histogram_quantile(0.95,
            rate(vendo_http_request_duration_seconds_bucket[5m])) > 0.5
        for: 5m
        labels:
          severity: warning
          team: platform
        annotations:
          summary: "High API response time"
          description: "P95 response time is {{ $value }}s (threshold: 500ms)"
```

**Circuit Breaker Open**:
```yaml
      - alert: CircuitBreakerOpen
        expr: vendo_circuit_breaker_state{state="open"} == 1
        for: 2m
        labels:
          severity: warning
          team: platform
        annotations:
          summary: "Circuit breaker is open"
          description: "Circuit breaker for {{ $labels.service }} -> {{ $labels.dependency }} is open"
```

**Low Inventory**:
```yaml
      - alert: LowInventory
        expr: vendo_product_inventory_quantity < 10
        labels:
          severity: warning
          team: operations
        annotations:
          summary: "Low inventory detected"
          description: "Product {{ $labels.product_id }} has only {{ $value }} units remaining"
```

#### 3.3.3 Application Insights Alert Configuration (MVP)

**Azure CLI**:
```bash
# High error rate alert
az monitor metrics alert create \
  --name "HighErrorRate" \
  --resource-group "vendo-prod" \
  --scopes "/subscriptions/.../resourceGroups/vendo-prod/providers/Microsoft.Insights/components/vendo-app-insights" \
  --condition "count exceptions > 10" \
  --window-size 5m \
  --evaluation-frequency 1m \
  --action "/subscriptions/.../resourceGroups/vendo-prod/providers/Microsoft.Insights/actionGroups/platform-team"

# High response time alert
az monitor metrics alert create \
  --name "HighResponseTime" \
  --resource-group "vendo-prod" \
  --scopes "/subscriptions/.../resourceGroups/vendo-prod/providers/Microsoft.Insights/components/vendo-app-insights" \
  --condition "avg requests/duration > 500" \
  --window-size 5m \
  --evaluation-frequency 1m \
  --action "/subscriptions/.../resourceGroups/vendo-prod/providers/Microsoft.Insights/actionGroups/platform-team"
```

---

### 3.4 Service Level Indicators (SLIs) and Objectives (SLOs)

#### 3.4.1 Availability SLO

**Target**: 99.5% uptime (43.8 minutes downtime per month)

**SLI Calculation**:
```promql
# Availability % (last 30 days)
100 * (
  1 - (
    sum(rate(vendo_http_errors_total{status_code=~"5.."}[30d]))
    /
    sum(rate(vendo_http_requests_total[30d]))
  )
)
```

**Error Budget**:
- Total requests per month (estimated): 10M
- Allowed errors (0.5%): 50,000
- Error budget consumed: `(actual_errors / 50000) * 100%`

**Grafana Dashboard Panel**:
```json
{
  "title": "Availability SLO (99.5%)",
  "targets": [
    {
      "expr": "100 * (1 - (sum(rate(vendo_http_errors_total{status_code=~\"5..\"}[30d])) / sum(rate(vendo_http_requests_total[30d]))))"
    }
  ],
  "thresholds": [
    {
      "value": 99.5,
      "color": "green"
    },
    {
      "value": 99.0,
      "color": "orange"
    },
    {
      "value": 98.0,
      "color": "red"
    }
  ]
}
```

#### 3.4.2 Latency SLO

**Target**: 95% of requests < 500ms

**SLI Calculation**:
```promql
# Percentage of requests under 500ms
100 * (
  sum(rate(vendo_http_request_duration_seconds_bucket{le="0.5"}[5m]))
  /
  sum(rate(vendo_http_request_duration_seconds_count[5m]))
)
```

**Per-Endpoint SLOs**:

| Endpoint | P95 Target | P99 Target | Rationale |
|----------|------------|------------|-----------|
| `GET /api/v1/products` | 200ms | 500ms | Fast browsing experience |
| `POST /api/v1/orders` | 500ms | 1000ms | Complex validation |
| `POST /api/v1/payments/checkout` | 1000ms | 2000ms | External API call |
| `GET /api/v1/orders/{id}` | 100ms | 300ms | Simple query |

#### 3.4.3 Payment Success Rate SLO

**Target**: 99% of valid payment attempts succeed

**SLI Calculation**:
```promql
100 * (
  sum(rate(vendo_payment_success_total[30d]))
  /
  sum(rate(vendo_payment_attempts_total[30d]))
)
```

**Exclusions**:
- User-initiated cancellations
- Invalid card errors (user fault)
- Expired checkout sessions

#### 3.4.4 SLO Monitoring Dashboard

**Implementation**:
```csharp
public class SloMetrics
{
    public Counter RequestsUnderThreshold { get; } = Metrics.CreateCounter(
        "vendo_slo_requests_under_threshold_total",
        "Requests that met SLO threshold",
        new CounterConfiguration { LabelNames = new[] { "service", "slo_type" } });

    public Counter RequestsTotal { get; } = Metrics.CreateCounter(
        "vendo_slo_requests_total",
        "Total requests measured for SLO",
        new CounterConfiguration { LabelNames = new[] { "service", "slo_type" } });

    public void RecordLatencySlo(string serviceName, TimeSpan duration)
    {
        RequestsTotal.WithLabels(serviceName, "latency").Inc();

        if (duration.TotalMilliseconds < 500)
        {
            RequestsUnderThreshold.WithLabels(serviceName, "latency").Inc();
        }
    }
}
```

---

## 4. Multi-Tenancy Implementation

### 4.1 Database Schema with Tenant Isolation

#### 4.1.1 Schema Design

**Principle**: Every table includes `TenantId` for logical isolation (shared database, shared schema approach for MVP).

**Base Entity**:
```csharp
public abstract class TenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}
```

**Example Tables**:

```sql
CREATE TABLE Products (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    SKU NVARCHAR(100) NOT NULL,
    Name NVARCHAR(500) NOT NULL,
    Description NVARCHAR(MAX),
    Price DECIMAL(18,2) NOT NULL,
    Currency NVARCHAR(3) NOT NULL,
    StockQuantity INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedBy NVARCHAR(256),

    -- Indexes (see section 4.2)
    INDEX IX_Products_TenantId_IsDeleted (TenantId, IsDeleted) INCLUDE (Name, Price, StockQuantity),
    INDEX IX_Products_TenantId_SKU (TenantId, SKU),
    INDEX IX_Products_TenantId_IsActive (TenantId, IsActive) WHERE IsDeleted = 0,

    -- Constraints
    CONSTRAINT UQ_Products_TenantId_SKU UNIQUE (TenantId, SKU),
    CONSTRAINT FK_Products_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT CK_Products_Price CHECK (Price >= 0),
    CONSTRAINT CK_Products_StockQuantity CHECK (StockQuantity >= 0)
);

CREATE TABLE Orders (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    CustomerId UNIQUEIDENTIFIER NOT NULL,
    OrderNumber NVARCHAR(50) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    Subtotal DECIMAL(18,2) NOT NULL,
    Tax DECIMAL(18,2) NOT NULL,
    Total DECIMAL(18,2) NOT NULL,
    Currency NVARCHAR(3) NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    INDEX IX_Orders_TenantId_CreatedAt (TenantId, CreatedAt DESC),
    INDEX IX_Orders_TenantId_CustomerId (TenantId, CustomerId),
    INDEX IX_Orders_TenantId_Status (TenantId, Status) WHERE IsDeleted = 0,
    INDEX IX_Orders_TenantId_OrderNumber (TenantId, OrderNumber),

    CONSTRAINT UQ_Orders_TenantId_OrderNumber UNIQUE (TenantId, OrderNumber),
    CONSTRAINT FK_Orders_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT CK_Orders_Totals CHECK (Total = Subtotal + Tax)
);
```

#### 4.1.2 EF Core Query Filters

**Configuration**:
```csharp
public class VendoDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public VendoDbContext(
        DbContextOptions<VendoDbContext> options,
        ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply tenant filter to all tenant entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(TenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(VendoDbContext)
                    .GetMethod(nameof(SetTenantFilter), BindingFlags.NonPublic | BindingFlags.Static)
                    .MakeGenericMethod(entityType.ClrType);

                method.Invoke(null, new object[] { modelBuilder, _tenantContext });
            }
        }
    }

    private static void SetTenantFilter<TEntity>(
        ModelBuilder modelBuilder,
        ITenantContext tenantContext)
        where TEntity : TenantEntity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e =>
            e.TenantId == tenantContext.TenantId && !e.IsDeleted);
    }

    public override int SaveChanges()
    {
        ApplyTenantId();
        ApplyAuditInfo();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyTenantId();
        ApplyAuditInfo();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyTenantId()
    {
        var entries = ChangeTracker.Entries<TenantEntity>()
            .Where(e => e.State == EntityState.Added);

        foreach (var entry in entries)
        {
            entry.Entity.TenantId = _tenantContext.TenantId;
        }
    }

    private void ApplyAuditInfo()
    {
        var userId = _tenantContext.UserId;
        var now = DateTime.UtcNow;

        var entries = ChangeTracker.Entries<TenantEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = userId;
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = userId;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = userId;
                    break;

                case EntityState.Deleted:
                    // Soft delete
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = userId;
                    break;
            }
        }
    }
}
```

---

### 4.2 Database Indexes for Multi-Tenant Performance

**Principle**: All indexes on tenant tables must include `TenantId` as the first column.

#### 4.2.1 Composite Index Strategy

**Products Table**:
```sql
-- Primary lookup: Get active products for tenant
CREATE NONCLUSTERED INDEX IX_Products_TenantId_IsActive
ON Products(TenantId, IsActive)
INCLUDE (Name, Price, StockQuantity, SKU)
WHERE IsDeleted = 0;

-- SKU lookup
CREATE NONCLUSTERED INDEX IX_Products_TenantId_SKU
ON Products(TenantId, SKU)
WHERE IsDeleted = 0;

-- Category filtering
CREATE NONCLUSTERED INDEX IX_Products_TenantId_CategoryId
ON Products(TenantId, CategoryId)
INCLUDE (Name, Price, IsActive)
WHERE IsDeleted = 0;

-- Full-text search (if needed)
CREATE FULLTEXT INDEX ON Products(Name, Description)
KEY INDEX PK_Products;
```

**Orders Table**:
```sql
-- Recent orders dashboard
CREATE NONCLUSTERED INDEX IX_Orders_TenantId_CreatedAt
ON Orders(TenantId, CreatedAt DESC)
INCLUDE (OrderNumber, Status, Total, Currency)
WHERE IsDeleted = 0;

-- Customer order history
CREATE NONCLUSTERED INDEX IX_Orders_TenantId_CustomerId_CreatedAt
ON Orders(TenantId, CustomerId, CreatedAt DESC)
INCLUDE (OrderNumber, Status, Total)
WHERE IsDeleted = 0;

-- Status filtering
CREATE NONCLUSTERED INDEX IX_Orders_TenantId_Status
ON Orders(TenantId, Status)
INCLUDE (OrderNumber, Total, CreatedAt)
WHERE IsDeleted = 0;

-- Order number lookup
CREATE UNIQUE NONCLUSTERED INDEX IX_Orders_TenantId_OrderNumber
ON Orders(TenantId, OrderNumber)
WHERE IsDeleted = 0;
```

**Payments Table**:
```sql
-- Order payment lookup
CREATE NONCLUSTERED INDEX IX_Payments_TenantId_OrderId
ON Payments(TenantId, OrderId)
INCLUDE (Amount, Status, ProviderTransactionId);

-- Provider transaction lookup (for webhook processing)
CREATE NONCLUSTERED INDEX IX_Payments_ProviderTransactionId
ON Payments(ProviderTransactionId)
INCLUDE (TenantId, OrderId, Status);

-- Recent payments
CREATE NONCLUSTERED INDEX IX_Payments_TenantId_CreatedAt
ON Payments(TenantId, CreatedAt DESC)
INCLUDE (OrderId, Amount, Status);
```

#### 4.2.2 Index Maintenance

**Weekly Index Maintenance Job**:
```sql
-- Rebuild fragmented indexes
DECLARE @TableName NVARCHAR(256);
DECLARE @IndexName NVARCHAR(256);
DECLARE @Fragmentation FLOAT;

DECLARE index_cursor CURSOR FOR
SELECT
    OBJECT_NAME(ips.object_id) AS TableName,
    i.name AS IndexName,
    ips.avg_fragmentation_in_percent
FROM sys.dm_db_index_physical_stats(
    DB_ID(), NULL, NULL, NULL, 'LIMITED') ips
INNER JOIN sys.indexes i
    ON ips.object_id = i.object_id
    AND ips.index_id = i.index_id
WHERE ips.avg_fragmentation_in_percent > 10
    AND i.name IS NOT NULL;

OPEN index_cursor;
FETCH NEXT FROM index_cursor INTO @TableName, @IndexName, @Fragmentation;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF @Fragmentation > 30
        EXEC('ALTER INDEX ' + @IndexName + ' ON ' + @TableName + ' REBUILD');
    ELSE
        EXEC('ALTER INDEX ' + @IndexName + ' ON ' + @TableName + ' REORGANIZE');

    FETCH NEXT FROM index_cursor INTO @TableName, @IndexName, @Fragmentation;
END;

CLOSE index_cursor;
DEALLOCATE index_cursor;

-- Update statistics
EXEC sp_updatestats;
```

---

### 4.3 Connection Pool Configuration

**Problem**: Multiple tenants sharing connection pool can cause contention.

**Solution**: Optimize pool settings for multi-tenant workload.

**Connection String Configuration**:
```json
{
  "ConnectionStrings": {
    "VendoDb": "Server=localhost;Database=VendoDb;User Id=vendo_app;Password=***;MultipleActiveResultSets=true;Min Pool Size=10;Max Pool Size=100;Connection Lifetime=300;Connect Timeout=30;Application Name=Vendo.OrderService;"
  }
}
```

**Parameter Explanation**:

| Parameter | Value | Rationale |
|-----------|-------|-----------|
| `Min Pool Size` | 10 | Keep warm connections ready for burst traffic |
| `Max Pool Size` | 100 | Limit total connections per service instance |
| `Connection Lifetime` | 300s | Recycle connections every 5 minutes to balance load |
| `Connect Timeout` | 30s | Fail fast if DB unavailable |
| `MultipleActiveResultSets` | true | Allow parallel queries in same connection |

**DbContext Configuration**:
```csharp
builder.Services.AddDbContext<VendoDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("VendoDb"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null);

            sqlOptions.CommandTimeout(30);

            // Performance optimizations
            sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        });

    // Development only
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Register as scoped (default) for request-scoped tenant context
// services.AddDbContext<VendoDbContext>(ServiceLifetime.Scoped);
```

**Monitoring Connection Pool**:
```csharp
public class ConnectionPoolMetrics
{
    public static void RegisterMetrics()
    {
        // .NET provides these via EventCounters
        // Monitor with dotnet-counters or Application Insights

        // Key metrics:
        // - Microsoft.Data.SqlClient.EventSource:
        //   - NumberOfActiveConnectionPools
        //   - NumberOfActiveConnections
        //   - NumberOfFreeConnections
        //   - NumberOfPooledConnections
    }
}
```

**AlertManager Rule**:
```yaml
- alert: ConnectionPoolExhaustion
  expr: |
    sqlserver_connection_pool_total - sqlserver_connection_pool_free < 5
  for: 2m
  labels:
    severity: warning
  annotations:
    summary: "Connection pool near exhaustion"
    description: "Only {{ $value }} free connections remaining in pool"
```

---

### 4.4 Query Performance Optimization Patterns

#### 4.4.1 Pagination

**Problem**: Loading all records causes memory issues and slow queries.

**Solution**: Always use pagination with `Skip` and `Take`.

**Implementation**:
```csharp
public class GetProductsQuery : IRequest<PagedResult<ProductDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string SearchTerm { get; init; }
    public Guid? CategoryId { get; init; }
    public bool? IsActive { get; init; }
}

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    public async Task<PagedResult<ProductDto>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Products.AsNoTracking(); // Read-only optimization

        // Filters
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = query.Where(p => p.Name.Contains(request.SearchTerm) ||
                                     p.SKU.Contains(request.SearchTerm));
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(p => p.IsActive == request.IsActive.Value);
        }

        // Total count (before pagination)
        var totalCount = await query.CountAsync(cancellationToken);

        // Pagination
        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                SKU = p.SKU,
                Name = p.Name,
                Price = p.Price,
                Currency = p.Currency,
                StockQuantity = p.StockQuantity,
                IsActive = p.IsActive
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductDto>
        {
            Items = products,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };
    }
}
```

#### 4.4.2 Projection (Select Only Required Fields)

**Problem**: Loading entire entities wastes memory and network bandwidth.

**Solution**: Use `.Select()` to project only needed fields.

```csharp
// ❌ BAD: Loads all columns
var products = await _context.Products.ToListAsync();

// ✅ GOOD: Loads only required columns
var products = await _context.Products
    .Select(p => new ProductListDto
    {
        Id = p.Id,
        Name = p.Name,
        Price = p.Price,
        StockQuantity = p.StockQuantity
    })
    .ToListAsync();
```

#### 4.4.3 Eager Loading (Avoid N+1 Queries)

**Problem**: Loading related entities in a loop causes N+1 query problem.

**Solution**: Use `.Include()` for eager loading.

```csharp
// ❌ BAD: N+1 queries
var orders = await _context.Orders.ToListAsync();
foreach (var order in orders)
{
    // Each iteration executes a separate query!
    var items = await _context.OrderItems
        .Where(i => i.OrderId == order.Id)
        .ToListAsync();
}

// ✅ GOOD: Single query with JOIN
var orders = await _context.Orders
    .Include(o => o.Items)
    .ToListAsync();

// ✅ BETTER: Projection with explicit join
var orders = await _context.Orders
    .Select(o => new OrderDto
    {
        Id = o.Id,
        OrderNumber = o.OrderNumber,
        Total = o.Total,
        Items = o.Items.Select(i => new OrderItemDto
        {
            ProductId = i.ProductId,
            Quantity = i.Quantity,
            Price = i.Price
        }).ToList()
    })
    .ToListAsync();
```

#### 4.4.4 AsNoTracking for Read-Only Queries

**Problem**: EF Core change tracking adds overhead for read-only queries.

**Solution**: Use `.AsNoTracking()` when you don't need to update entities.

```csharp
// ✅ Read-only query
var products = await _context.Products
    .AsNoTracking()
    .Where(p => p.IsActive)
    .ToListAsync();

// ❌ Don't use AsNoTracking if you plan to update
var product = await _context.Products
    .AsNoTracking() // BAD!
    .FirstOrDefaultAsync(p => p.Id == productId);

product.Price = 99.99m;
await _context.SaveChangesAsync(); // Won't save changes!
```

#### 4.4.5 Compiled Queries (for frequently executed queries)

**Problem**: LINQ query compilation overhead on every execution.

**Solution**: Use compiled queries for hot paths.

```csharp
public static class CompiledQueries
{
    public static readonly Func<VendoDbContext, Guid, Guid, Task<Product>> GetProductById =
        EF.CompileAsyncQuery((VendoDbContext context, Guid tenantId, Guid productId) =>
            context.Products
                .FirstOrDefault(p => p.TenantId == tenantId && p.Id == productId));

    public static readonly Func<VendoDbContext, Guid, string, Task<Product>> GetProductBySku =
        EF.CompileAsyncQuery((VendoDbContext context, Guid tenantId, string sku) =>
            context.Products
                .FirstOrDefault(p => p.TenantId == tenantId && p.SKU == sku));
}

// Usage
var product = await CompiledQueries.GetProductById(_context, tenantId, productId);
```

---

### 4.5 Tenant Quotas and Rate Limiting

**Decision**: Implement tiered quotas based on subscription level.

#### 4.5.1 Quota Definition

**Free Tier**:
```json
{
  "tier": "free",
  "limits": {
    "maxProducts": 100,
    "maxOrders": 50,
    "maxStorageGB": 1,
    "maxApiRequestsPerHour": 1000,
    "maxConcurrentWebhooks": 5
  }
}
```

**Pro Tier**:
```json
{
  "tier": "pro",
  "limits": {
    "maxProducts": 10000,
    "maxOrders": 5000,
    "maxStorageGB": 50,
    "maxApiRequestsPerHour": 10000,
    "maxConcurrentWebhooks": 50
  }
}
```

**Enterprise Tier**:
```json
{
  "tier": "enterprise",
  "limits": {
    "maxProducts": -1,
    "maxOrders": -1,
    "maxStorageGB": 500,
    "maxApiRequestsPerHour": 100000,
    "maxConcurrentWebhooks": 200
  }
}
```

#### 4.5.2 Quota Enforcement

**Domain Service**:
```csharp
public interface IQuotaService
{
    Task<QuotaCheckResult> CheckQuotaAsync(
        Guid tenantId,
        QuotaType quotaType,
        int requestedAmount = 1);

    Task<TenantQuotaStatus> GetQuotaStatusAsync(Guid tenantId);
}

public class QuotaService : IQuotaService
{
    private readonly VendoDbContext _context;
    private readonly IMemoryCache _cache;

    public async Task<QuotaCheckResult> CheckQuotaAsync(
        Guid tenantId,
        QuotaType quotaType,
        int requestedAmount = 1)
    {
        var tenant = await GetTenantWithQuotasAsync(tenantId);
        var limit = GetQuotaLimit(tenant.SubscriptionTier, quotaType);

        if (limit == -1) // Unlimited
            return QuotaCheckResult.Success();

        var currentUsage = await GetCurrentUsageAsync(tenantId, quotaType);

        if (currentUsage + requestedAmount > limit)
        {
            return QuotaCheckResult.Exceeded(
                quotaType,
                currentUsage,
                limit,
                $"Quota exceeded for {quotaType}. Current: {currentUsage}, Limit: {limit}");
        }

        return QuotaCheckResult.Success();
    }

    private async Task<int> GetCurrentUsageAsync(Guid tenantId, QuotaType quotaType)
    {
        // Cache quota usage for 5 minutes
        var cacheKey = $"quota:{tenantId}:{quotaType}";
        if (_cache.TryGetValue(cacheKey, out int cachedUsage))
            return cachedUsage;

        var usage = quotaType switch
        {
            QuotaType.Products => await _context.Products
                .CountAsync(p => p.TenantId == tenantId),

            QuotaType.Orders => await _context.Orders
                .CountAsync(o => o.TenantId == tenantId),

            QuotaType.Storage => await CalculateStorageUsageAsync(tenantId),

            _ => throw new ArgumentException($"Unknown quota type: {quotaType}")
        };

        _cache.Set(cacheKey, usage, TimeSpan.FromMinutes(5));
        return usage;
    }
}
```

**Usage in Handler**:
```csharp
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductResult>
{
    private readonly IQuotaService _quotaService;

    public async Task<ProductResult> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        // Check quota before creating
        var quotaCheck = await _quotaService.CheckQuotaAsync(
            request.TenantId,
            QuotaType.Products);

        if (!quotaCheck.IsSuccess)
        {
            throw new QuotaExceededException(quotaCheck.Message);
        }

        // Proceed with creation
        var product = Product.Create(/* ... */);
        await _repository.AddAsync(product, cancellationToken);

        return new ProductResult(product.Id);
    }
}
```

---

## 5. API Gateway Architecture

### 5.1 Technology Choice: YARP (Yet Another Reverse Proxy)

**Decision**: Use **YARP** (Microsoft's reverse proxy) as API Gateway for MVP.

**Rationale**:
- **Free and Open Source**: No licensing costs
- **High Performance**: Built on ASP.NET Core Kestrel
- **.NET Native**: Seamless integration with ASP.NET Core middleware
- **Flexible**: Supports custom routing, transformations, load balancing
- **Solo-Friendly**: Simple YAML/JSON configuration
- **Production-Ready**: Used by Microsoft internally

**Alternative Considered**: Ocelot, Kong, Nginx
- **Rejected because**: YARP offers better .NET integration and performance

**NuGet Package**:
```xml
<PackageReference Include="Yarp.ReverseProxy" Version="2.0.1" />
```

---

### 5.2 YARP Configuration

**Project Structure**:
```
/src
 └── ApiGateway
     ├── Program.cs
     ├── appsettings.json
     ├── Middleware/
     │   ├── TenantResolutionMiddleware.cs
     │   ├── RateLimitingMiddleware.cs
     │   └── AuthenticationMiddleware.cs
     └── Transformations/
         └── TenantHeaderTransformation.cs
```

**Configuration** (`appsettings.json`):
```json
{
  "ReverseProxy": {
    "Routes": {
      "products-route": {
        "ClusterId": "catalog-service",
        "Match": {
          "Path": "/api/v1/products/{**catch-all}"
        },
        "Transforms": [
          {
            "RequestHeader": "X-Tenant-ID",
            "Set": "{tenant_id}"
          },
          {
            "PathPattern": "/api/v1/products/{**catch-all}"
          }
        ]
      },
      "orders-route": {
        "ClusterId": "order-service",
        "Match": {
          "Path": "/api/v1/orders/{**catch-all}"
        },
        "Transforms": [
          {
            "RequestHeader": "X-Tenant-ID",
            "Set": "{tenant_id}"
          }
        ]
      },
      "payments-route": {
        "ClusterId": "payment-service",
        "Match": {
          "Path": "/api/v1/payments/{**catch-all}"
        },
        "RateLimiterPolicy": "payment-limiter"
      },
      "auth-route": {
        "ClusterId": "auth-service",
        "Match": {
          "Path": "/connect/{**catch-all}"
        },
        "RateLimiterPolicy": "auth-limiter"
      }
    },
    "Clusters": {
      "catalog-service": {
        "Destinations": {
          "destination1": {
            "Address": "https://localhost:5003"
          }
        },
        "HealthCheck": {
          "Active": {
            "Enabled": true,
            "Interval": "00:00:30",
            "Timeout": "00:00:10",
            "Policy": "ConsecutiveFailures",
            "Path": "/health"
          }
        }
      },
      "order-service": {
        "Destinations": {
          "destination1": {
            "Address": "https://localhost:5004"
          }
        },
        "HealthCheck": {
          "Active": {
            "Enabled": true,
            "Interval": "00:00:30",
            "Path": "/health"
          }
        }
      },
      "payment-service": {
        "Destinations": {
          "destination1": {
            "Address": "https://localhost:5005"
          }
        }
      },
      "auth-service": {
        "Destinations": {
          "destination1": {
            "Address": "https://localhost:5001"
          }
        }
      }
    }
  }
}
```

**Startup Configuration** (`Program.cs`):
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms<TenantHeaderTransformation>();

// Add custom services
builder.Services.AddSingleton<ITenantResolver, TenantResolver>();
builder.Services.AddMemoryCache();

// Add rate limiting
builder.Services.AddRateLimiter(options =>
{
    // Per-tenant rate limiting
    options.AddPolicy("tenant-limiter", context =>
    {
        var tenantId = context.Request.Headers["X-Tenant-ID"].FirstOrDefault();
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: tenantId ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            });
    });

    // Payment endpoint stricter limits
    options.AddPolicy("payment-limiter", context =>
    {
        var tenantId = context.Request.Headers["X-Tenant-ID"].FirstOrDefault();
        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: tenantId ?? "anonymous",
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6
            });
    });

    // Auth endpoint limits (prevent brute force)
    options.AddPolicy("auth-limiter", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ip,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1)
            });
    });
});

var app = builder.Build();

// Middleware pipeline
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// YARP reverse proxy
app.MapReverseProxy();

app.Run();
```

---

### 5.3 Rate Limiting

#### 5.3.1 Tenant-Based Rate Limits

**Configuration**:
```csharp
public class TenantRateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TenantRateLimitingMiddleware> _logger;

    // Rate limit configurations by tier
    private static readonly Dictionary<string, RateLimitConfig> TierLimits = new()
    {
        ["free"] = new RateLimitConfig
        {
            RequestsPerMinute = 100,
            RequestsPerHour = 1000,
            BurstSize = 20
        },
        ["pro"] = new RateLimitConfig
        {
            RequestsPerMinute = 1000,
            RequestsPerHour = 10000,
            BurstSize = 200
        },
        ["enterprise"] = new RateLimitConfig
        {
            RequestsPerMinute = 10000,
            RequestsPerHour = 100000,
            BurstSize = 2000
        }
    };

    public async Task InvokeAsync(HttpContext context)
    {
        var tenantId = context.Request.Headers["X-Tenant-ID"].FirstOrDefault();
        if (string.IsNullOrEmpty(tenantId))
        {
            await _next(context);
            return;
        }

        var tier = await GetTenantTierAsync(tenantId);
        var limits = TierLimits.GetValueOrDefault(tier, TierLimits["free"]);

        // Check rate limits
        var minuteKey = $"ratelimit:{tenantId}:minute";
        var hourKey = $"ratelimit:{tenantId}:hour";

        var minuteCount = _cache.GetOrCreate(minuteKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            return 0;
        });

        var hourCount = _cache.GetOrCreate(hourKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            return 0;
        });

        if (minuteCount >= limits.RequestsPerMinute || hourCount >= limits.RequestsPerHour)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers["Retry-After"] = "60";
            context.Response.Headers["X-RateLimit-Limit"] = limits.RequestsPerMinute.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = "0";

            await context.Response.WriteAsJsonAsync(new
            {
                error = "rate_limit_exceeded",
                message = "Too many requests. Please try again later."
            });

            return;
        }

        // Increment counters
        _cache.Set(minuteKey, minuteCount + 1);
        _cache.Set(hourKey, hourCount + 1);

        // Add rate limit headers
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-RateLimit-Limit"] = limits.RequestsPerMinute.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] =
                (limits.RequestsPerMinute - minuteCount - 1).ToString();
            return Task.CompletedTask;
        });

        await _next(context);
    }
}
```

#### 5.3.2 Per-User Rate Limits

**Configuration** (for logged-in users):
```csharp
public class UserRateLimitingMiddleware
{
    private const int RequestsPerSecond = 10;
    private const int BurstSize = 20;

    public async Task InvokeAsync(HttpContext context)
    {
        var userId = context.User?.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            await _next(context);
            return;
        }

        var key = $"ratelimit:user:{userId}:second";
        var count = _cache.GetOrCreate(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(1);
            return 0;
        });

        if (count >= RequestsPerSecond)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "rate_limit_exceeded",
                message = "Too many requests per second"
            });
            return;
        }

        _cache.Set(key, count + 1);
        await _next(context);
    }
}
```

---

### 5.4 API Versioning Strategy

**Decision**: Use **URL-based versioning** with `/api/v{version}/` prefix.

**Rationale**:
- **Explicit**: Version is clearly visible in URL
- **Gateway-Friendly**: Easy to route different versions to different services
- **Caching-Friendly**: Different URLs for different versions
- **Simple**: No custom headers or query parameters required

**URL Format**:
```
/api/v1/products
/api/v1/orders
/api/v2/products  (future breaking changes)
```

#### 5.4.1 Versioning Configuration

**YARP Route Configuration**:
```json
{
  "Routes": {
    "products-v1": {
      "ClusterId": "catalog-service-v1",
      "Match": {
        "Path": "/api/v1/products/{**catch-all}"
      }
    },
    "products-v2": {
      "ClusterId": "catalog-service-v2",
      "Match": {
        "Path": "/api/v2/products/{**catch-all}"
      }
    }
  },
  "Clusters": {
    "catalog-service-v1": {
      "Destinations": {
        "destination1": {
          "Address": "https://localhost:5003"
        }
      }
    },
    "catalog-service-v2": {
      "Destinations": {
        "destination1": {
          "Address": "https://localhost:5013"
        }
      }
    }
  }
}
```

**Service-Side Configuration** (ASP.NET Core):
```csharp
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

// Controllers
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts(
        [FromQuery] GetProductsQuery query)
    {
        // v1 implementation
    }
}

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("2.0")]
public class ProductsV2Controller : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDtoV2>>> GetProducts(
        [FromQuery] GetProductsQueryV2 query)
    {
        // v2 implementation with breaking changes
    }
}
```

**Version Deprecation Headers**:
```csharp
public class ApiVersionDeprecationMiddleware
{
    private static readonly HashSet<string> DeprecatedVersions = new() { "1.0" };

    public async Task InvokeAsync(HttpContext context)
    {
        var apiVersion = context.GetRequestedApiVersion()?.ToString();

        if (apiVersion != null && DeprecatedVersions.Contains(apiVersion))
        {
            context.Response.Headers["X-API-Deprecated"] = "true";
            context.Response.Headers["X-API-Sunset-Date"] = "2026-12-31";
            context.Response.Headers["Link"] = "</api/v2>; rel=\"successor-version\"";
        }

        await _next(context);
    }
}
```

---

## Summary

This document has specified:

1. **Technology Decisions**:
   - OpenIddict for auth (free OSS alternative to Duende)
   - Stripe for payments (MVP)
   - Application Insights → Prometheus + Grafana (phased)

2. **Distributed Systems**:
   - Service boundaries and communication patterns
   - Polly resilience policies (circuit breaker, retry, timeout)
   - RabbitMQ for async messaging
   - OpenTelemetry for distributed tracing
   - Saga pattern for order-payment orchestration

3. **Observability**:
   - RED metrics for all services
   - Structured logging with Serilog
   - Alerting rules and thresholds
   - SLIs/SLOs (99.5% uptime, <500ms P95)

4. **Multi-Tenancy**:
   - Database schema with TenantId
   - Composite indexes for performance
   - Connection pool optimization
   - Query patterns (pagination, projection, AsNoTracking)
   - Tenant quotas enforcement

5. **API Gateway**:
   - YARP configuration
   - Rate limiting (100 req/min per tenant, 10/s per user)
   - URL-based versioning (/api/v1/)

**Next Document**: Part 2 will cover data architecture, CI/CD pipelines, deployment strategies, and security hardening.
