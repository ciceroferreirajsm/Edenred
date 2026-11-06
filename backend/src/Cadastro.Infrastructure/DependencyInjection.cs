using Cadastro.Application.Abstractions;
using Cadastro.Infrastructure.Persistence;
using Cadastro.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cadastro.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? "Data Source=cadastro.db";

        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IPessoaRepository, PessoaRepository>();
        services.AddScoped<IVeiculoRepository, VeiculoRepository>();

        return services;
    }
}
