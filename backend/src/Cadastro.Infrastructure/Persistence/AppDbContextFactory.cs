using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Cadastro.Infrastructure.Persistence;

/// <summary>
/// Usada apenas em tempo de design pelo <c>dotnet ef</c> (gerar migrations / scripts).
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=cadastro.db")
            .Options;

        return new AppDbContext(options);
    }
}
