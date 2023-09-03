using FluentAssertions;
using Microsoft.Data.SqlClient;
using WillSoss.DbDeploy.Sql;

namespace WillSoss.DbDeploy.Tests
{
    [Collection(nameof(DatabaseCollection))]
    [Trait("Category", "Migrations")]
    public class MigrateTests : DatabaseTest
    {

        public MigrateTests(DatabaseFixture fixture)
            : base(fixture, "test") { }
        
        [Fact]
        public void ShouldLoadMigrations()
        {
            // Arrange
            var migrationsPath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts");

            // Act
            var db = SqlDatabase.CreateBuilder()
                .WithConnectionString(ConnectionString)
                .AddMigrations(migrationsPath)
                .Build();

            // Assert
            db.Migrations.Count().Should().Be(10);
        }

        [Fact]
        public async void ShouldApplyMigrations()
        {
            // Arrange
            var migrationsPath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts");

            var db = SqlDatabase.CreateBuilder()
                .WithConnectionString(ConnectionString)
                .AddMigrations(migrationsPath)
                .Build();

            await db.Create();

            // Act
            var count = await db.MigrateToLatest();

            // Assert
            count.Should().Be(10);

            var migrations = await db.GetAppliedMigrations(db.GetConnection());

            // 4 scripts + database create
            migrations.Count().Should().Be(11);
        }

        [Fact]
        public async void ShouldApplyMigrationToVersion()
        {
            // Arrange
            var migrationsPath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts");

            var db = SqlDatabase.CreateBuilder()
                .WithConnectionString(ConnectionString)
                .AddMigrations(migrationsPath)
                .Build();

            await db.Create();

            await db.MigrateTo(new Version(0, 1));

            // Act
            var count = await db.MigrateToLatest();

            // Assert
            count.Should().Be(6);

            var migrations = await db.GetAppliedMigrations(db.GetConnection());

            // 10 scripts + database create
            migrations.Count().Should().Be(11);
        }

        [Fact]
        public async void ShouldNotApplySkippedMigrations()
        {
            // Arrange

            // Apply migrations, but skip "1.1 one-one.sql"
            var migrationsPath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts - One Missing");

            var db = SqlDatabase.CreateBuilder()
                .WithConnectionString(ConnectionString)
                .AddMigrations(migrationsPath)
                .Build();

            await db.Create();

            await db.MigrateToLatest();

            // Now set up for migrating again, but with the missing script
            migrationsPath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts");

            db = SqlDatabase.CreateBuilder()
                .WithConnectionString(ConnectionString)
                .AddMigrations(migrationsPath)
                .Build();

            // Act
            var ex = await Assert.ThrowsAsync<MigrationsNotAppliedInOrderException>(db.MigrateToLatest);

            // Assert
            ex.Should().NotBeNull();
        }
    }
}
