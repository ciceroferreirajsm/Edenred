using System.Reflection;
using Cadastro.Api.Middlewares;
using Cadastro.Application;
using Cadastro.Infrastructure;
using Cadastro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddControllers(options =>
{
    // As regras de obrigatoriedade ficam no FluentValidation (mensagens em PT-BR e padronizadas).
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = ctx =>
        ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddHealthChecks();

const string CorsPolicy = "Frontend";
builder.Services.AddCors(options =>
{
    var origens = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:4200"];
    options.AddPolicy(CorsPolicy, policy => policy.WithOrigins(origens).AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Cadastro de Pessoas e Veículos",
        Version = "v1",
        Description = "API REST para cadastro de pessoas e seus veículos."
    });

    foreach (var xml in new[] { Assembly.GetExecutingAssembly(), typeof(Cadastro.Application.DependencyInjection).Assembly }
                 .Select(a => Path.Combine(AppContext.BaseDirectory, $"{a.GetName().Name}.xml"))
                 .Where(File.Exists))
    {
        options.IncludeXmlComments(xml);
    }
});

var app = builder.Build();

if (app.Configuration.GetValue("Database:AplicarMigrationsNaInicializacao", true))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
}

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Cadastro v1");
    options.DocumentTitle = "Cadastro de Pessoas e Veículos - API";
});

app.UseCors(CorsPolicy);

app.MapControllers();
app.MapHealthChecks("/health");
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

await app.RunAsync();

/// <summary>Exposto para os testes de integração (WebApplicationFactory).</summary>
public partial class Program;
