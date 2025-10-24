using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Vendo.Payment.Application;
using Vendo.Payment.Application.Commands.CreatePayment;
using Vendo.Payment.Infrastructure;
using Vendo.Payment.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to use specific port
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(5004, listenOptions =>
    {
        listenOptions.UseHttps();
    });
});

// Add services to the container
builder.Services.AddControllers();

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreatePaymentCommand).Assembly));

// Add FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(CreatePaymentCommand).Assembly);

// Add Application and Infrastructure services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Vendo Payment Service API",
        Version = "v1",
        Description = "Payment processing service for Vendo platform"
    });
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
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Vendo Payment Service API V1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
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
        var context = services.GetRequiredService<PaymentDbContext>();
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
