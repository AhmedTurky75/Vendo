using Microsoft.EntityFrameworkCore;
using Vendo.Catalog.Application;
using Vendo.Catalog.Infrastructure;
using Vendo.Catalog.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to listen on port 5002
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5002, listenOptions =>
    {
        listenOptions.UseHttps();
    });
});

// Add services to the container
builder.Services.AddControllers();

// Add Application and Infrastructure services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Vendo Catalog API",
        Version = "v1",
        Description = "Catalog Service for Vendo - Product and Category Management",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Vendo Development Team"
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

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Vendo Catalog API v1");
        options.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// Apply migrations and seed data on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        // Apply pending migrations automatically
        var context = services.GetRequiredService<CatalogDbContext>();
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

app.Run();
