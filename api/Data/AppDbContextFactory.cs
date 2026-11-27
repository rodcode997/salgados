using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Salgados.Api.Data;

/// <summary>
/// Garante que comandos `dotnet ef migrations` sempre gerem migrations para SQL Server.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=SalgadosDb;Trusted_Connection=True;TrustServerCertificate=True;")
            .Options;

        return new AppDbContext(options);
    }
}
