using FluentValidation.AspNetCore;
using Microsoft.OpenApi.Models;
using Serilog;
using SmartInvoicePro.API.Middleware;
using SmartInvoicePro.Application;
using SmartInvoicePro.Infrastructure;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .Build())
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Starting SmartInvoice Pro API");

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    ValidateJwtConfiguration(builder);

    builder.Services.AddApplication();
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "SmartInvoice Pro API", Version = "v1" });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
    });

    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:4200"];
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AngularApp", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<SmartInvoicePro.Application.Interfaces.IDataSeeder>();
        await seeder.SeedAsync();
    }

    app.UseMiddleware<ExceptionHandlingMiddleware>();

    if (app.Environment.IsDevelopment() || builder.Configuration.GetValue("Swagger:Enabled", true))
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();
    app.UseCors("AngularApp");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

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

static void ValidateJwtConfiguration(WebApplicationBuilder builder)
{
    var jwtKey = builder.Configuration["Jwt:Key"];
    var isMissing = string.IsNullOrWhiteSpace(jwtKey)
        || jwtKey.Contains("REPLACE_ME", StringComparison.OrdinalIgnoreCase);

    if (!isMissing)
        return;

    if (builder.Environment.IsDevelopment())
        throw new InvalidOperationException(
            "Jwt:Key is missing. Set it in appsettings.Development.json or via Jwt__Key.");

    throw new InvalidOperationException(
        "Jwt:Key must be set via environment variable Jwt__Key (or configuration) in non-Development environments. Do not commit production secrets.");
}
