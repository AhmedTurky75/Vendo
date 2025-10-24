using Duende.Bff;
using Duende.Bff.Yarp;

var builder = WebApplication.CreateBuilder(args);

// Add CORS configuration for Angular Merchant Portal (mfe-merchant on port 4203 and shell-app on port 4200)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "https://localhost:4203", "http://localhost:4203",  // Merchant MFE
                "https://localhost:4200", "http://localhost:4200"   // Shell App
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Important for cookies
    });
});

// Configure cookie authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "cookie";
    options.DefaultChallengeScheme = "oidc";
    options.DefaultSignOutScheme = "oidc";
})
.AddCookie("cookie", options =>
{
    options.Cookie.Name = "__Host-merchant-bff";
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

    // Session timeout - 60 minutes idle for merchant portal (longer than admin)
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;

    // Path restrictions
    options.Cookie.Path = "/";
})
.AddOpenIdConnect("oidc", options =>
{
    options.Authority = "https://localhost:5001";

    // Client configuration
    options.ClientId = "merchant-bff";
    options.ClientSecret = ""; // No secret for public client
    options.ResponseType = "code";
    options.ResponseMode = "query";

    // Use PKCE
    options.UsePkce = true;

    // Scopes
    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.Scope.Add("roles");
    options.Scope.Add("tenant");
    options.Scope.Add("vendo.api.full_access");
    options.Scope.Add("offline_access"); // For refresh tokens

    // Token management
    options.GetClaimsFromUserInfoEndpoint = true;
    options.SaveTokens = true;
    options.MapInboundClaims = false; // Keep original claim types

    // For development only
    options.RequireHttpsMetadata = false;
});

// Add BFF services
builder.Services.AddBff()
    .AddRemoteApis(); // Required for YARP integration

// Add authorization
builder.Services.AddAuthorization();

// Configure YARP reverse proxy for backend APIs
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddBffExtensions();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors();

// Routing must be before auth
app.UseRouting();

// Enable authentication & authorization
app.UseAuthentication();

// Enable BFF middleware (includes anti-forgery)
app.UseBff();

app.UseAuthorization();

// Map BFF management endpoints
app.MapBffManagementEndpoints();

// Map YARP routes for backend API proxying
app.MapReverseProxy(proxyPipeline =>
{
    // Add BFF token management to outgoing requests
    proxyPipeline.UseAntiforgeryCheck();
});

// Health check
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = "Merchant BFF",
    timestamp = DateTime.UtcNow
}))
.WithName("HealthCheck");

app.Run();
