using Microsoft.EntityFrameworkCore;
using Serilog;
using Vendo.TenantManagement.Api.Middleware;
using Vendo.TenantManagement.Application;
using Vendo.TenantManagement.Infrastructure;
using Vendo.TenantManagement.Infrastructure.Persistence;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/tenant-management-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting Tenant Management API");

    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog();

    // Add services to the container
    builder.Services.AddControllers();

    // Add Application layer
    builder.Services.AddApplication();

    // Add Infrastructure layer
    builder.Services.AddInfrastructure(builder.Configuration);

    // Add API documentation
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Vendo Tenant Management API",
            Version = "v1",
            Description = "API for managing stores/tenants in the Vendo multi-tenant e-commerce platform",
            Contact = new Microsoft.OpenApi.Models.OpenApiContact
            {
                Name = "Vendo Support",
                Email = "support@vendo.app"
            }
        });

        // Include XML comments
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    });

    // Add CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    // Add health checks
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<Vendo.TenantManagement.Infrastructure.Persistence.TenantManagementDbContext>();

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tenant Management API v1");
            c.RoutePrefix = string.Empty; // Serve Swagger UI at root
        });
    }

    // Global exception handling
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    app.UseSerilogRequestLogging();

    app.UseHttpsRedirection();

    app.UseCors("AllowAll");

    app.UseAuthorization();

    app.MapControllers();

    app.MapHealthChecks("/health");

    // Apply migrations and seed data on startup
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            // Apply pending migrations automatically
            var context = services.GetRequiredService<TenantManagementDbContext>();
            logger.LogInformation("Applying database migrations...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully.");

            // Seed initial data
            logger.LogInformation("Starting database seeding...");
            var seeder = new DataSeeder(context, services.GetRequiredService<ILogger<DataSeeder>>());
            await seeder.SeedAsync();
            logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating or seeding the database.");
            // Don't throw - allow the application to start even if seeding fails
        }
    }

    Log.Information("Tenant Management API started successfully");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
