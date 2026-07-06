using Anemoi.BuildingBlock.Infrastructure.RunSqlMigration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Xunit;

namespace Anemoi.BuildingBlock.Test;

public sealed class MigrationDatabaseTests
{
    [Fact]
    public async Task MigrationDatabaseAsync_WhenMigrationFails_ThrowsToStopStartup()
    {
        using var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton(Log.Logger);
                services.AddDbContext<FailingMigrationDbContext>(options =>
                    options.UseNpgsql("Host=127.0.0.1;Port=1;Username=postgres;Password=postgres;Database=MissingDatabase;Timeout=1;Command Timeout=1"));
            })
            .Build();

        await Assert.ThrowsAnyAsync<Exception>(() =>
            MigrationDatabase.MigrationDatabaseAsync<FailingMigrationDbContext>(host));
    }

    private sealed class FailingMigrationDbContext(DbContextOptions<FailingMigrationDbContext> options) : DbContext(options)
    {
        public DbSet<MigrationProbe> MigrationProbes => Set<MigrationProbe>();
    }

    private sealed class MigrationProbe
    {
        public int Id { get; set; }
    }
}
