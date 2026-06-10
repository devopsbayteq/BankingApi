using BankingApi.Middleware;
using BankingApi.Services;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------------ //
//  Services
// ------------------------------------------------------------------ //

builder.Services.AddControllers();
builder.Services.AddScoped<ITransactionService, MockTransactionService>();

// OpenAPI / Swagger (OpenAPI 3.0)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "Banking Transactions API",
        Version     = "v1",
        Description = "Mock REST API that exposes account movements. Compliant with OpenAPI 3.0.",
        Contact     = new OpenApiContact
        {
            Name  = "DevOps Banking Team",
            Email = "devops@bankingapi.example.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT",
            Url  = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Include XML documentation comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);

    options.EnableAnnotations();

    // Security definition placeholder (Bearer JWT) — no real auth in mock
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In          = ParameterLocation.Header,
        Description = "JWT Authorization header. Example: 'Bearer {token}'",
        Name        = "Authorization",
        Type        = SecuritySchemeType.Http,
        Scheme      = "bearer",
        BearerFormat = "JWT"
    });
});

// Health checks (used by Elastic Beanstalk health monitoring)
builder.Services.AddHealthChecks();

// ------------------------------------------------------------------ //
//  Application pipeline
// ------------------------------------------------------------------ //

var app = builder.Build();

app.UseGlobalExceptionHandler();

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "no-referrer");
    context.Response.Headers.Append("Permissions-Policy", "geolocation=()");
    await next();
});

app.UseSwagger(c =>
{
    c.RouteTemplate = "api-docs/{documentName}/swagger.json";
});

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/api-docs/v1/swagger.json", "Banking API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Make Program accessible to integration tests
public partial class Program { }
