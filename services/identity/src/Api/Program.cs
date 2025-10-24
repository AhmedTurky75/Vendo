using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Vendo.Identity.Api.Middleware;
using Vendo.Identity.Application;
using Vendo.Identity.Infrastructure;
using Vendo.Identity.Infrastructure.Persistence;
using Vendo.Identity.Application.Common.Interfaces;
using Vendo.Identity.Domain.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Application layer services
builder.Services.AddApplicationServices();

// Configure Infrastructure layer services (includes IdentityServer)
builder.Services.AddInfrastructureServices();

// Configure JWT Bearer authentication for API endpoints
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://localhost:5001";
        options.Audience = "vendo.api";
        options.RequireHttpsMetadata = false; // For development only
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidTypes = new[] { "at+jwt" }
        };
    });

// Configure authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserPolicy", policy => policy.RequireRole("User", "Admin"));
});

// Configure Swagger with OAuth2 support
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Vendo Identity API",
        Version = "v1",
        Description = "Identity and authentication service for Vendo platform using Duende IdentityServer",
        Contact = new OpenApiContact
        {
            Name = "Vendo Team",
            Email = "support@vendo.com"
        }
    });

    // Add XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // Configure OAuth2 authentication in Swagger
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri("https://localhost:5001/connect/authorize"),
                TokenUrl = new Uri("https://localhost:5001/connect/token"),
                Scopes = new Dictionary<string, string>
                {
                    { "openid", "OpenID Connect" },
                    { "profile", "User profile" },
                    { "email", "User email" },
                    { "vendo.api.full_access", "Full access to Vendo API" },
                    { "roles", "User roles" }
                }
            }
        }
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "oauth2"
                }
            },
            new[] { "vendo.api.full_access" }
        }
    });
});

// Configure CORS for proper security
// In production, these should come from configuration
builder.Services.AddCors(options =>
{
    // Default policy for API access
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] {
                "https://localhost:5001", // Identity Service
                "https://localhost:5002", // Web App
                "https://localhost:5101", // Admin BFF
                "https://localhost:5102", // Merchant BFF
                "https://localhost:4200", // Customer Portal
                "https://localhost:4300", // Admin Portal
                "https://localhost:4400"  // Merchant Portal
            };

        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });

    // Strict policy for IdentityServer endpoints
    options.AddPolicy("IdentityServerPolicy", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] {
                "https://localhost:5001", // Identity Service
                "https://localhost:5002", // Web App
                "https://localhost:5101", // Admin BFF
                "https://localhost:5102", // Merchant BFF
                "https://localhost:4200", // Customer Portal
                "https://localhost:4300", // Admin Portal
                "https://localhost:4400"  // Merchant Portal
            };

        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .WithMethods("GET", "POST")
              .AllowCredentials()
              .WithExposedHeaders("WWW-Authenticate");
    });
});

var app = builder.Build();

// Seed initial data
using (var scope = app.Services.CreateScope())
{
    var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await SeedData.SeedUsersAsync(userRepository, passwordHasher);
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Vendo Identity API v1");
        options.OAuthClientId("swagger");
        options.OAuthAppName("Swagger UI");
        options.OAuthUsePkce();
    });
}

// Add custom middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

// Enable CORS
app.UseCors();

// Enable IdentityServer middleware
app.UseIdentityServer();

// Enable authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck")
    .WithOpenApi();

app.Run();
