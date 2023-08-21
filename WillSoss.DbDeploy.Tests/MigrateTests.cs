using FluentAssertions;
using Microsoft.Data.SqlClient;
using System.CommandLine.Parsing;
using WillSoss.DbDeploy.Sql;

namespace WillSoss.DbDeploy.Tests
{
    public class MigrateTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly IntegrationTestFixture _fixture;

        public MigrateTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void ShouldLoadMigrations()
        {
            // Arrange
            var migrationsPath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts");

            var cs = new SqlConnectionStringBuilder(_fixture.DbContainer.ConnectionString)
            {
                InitialCatalog = "test"
            };

            // Act
            var db = SqlDatabase.CreateBuilder()
                .WithConnectionString(cs.ToString())
                .AddMigrations(migrationsPath)
                .Build();

            // Assert
            db.Migrations.Count().Should().Be(4);
        }

        [Fact]
        public async void ShouldApplyMigrations()
        {
            // Arrange
            var migrationsPath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts");

            var cs = new SqlConnectionStringBuilder(_fixture.DbContainer.ConnectionString)
            {
                InitialCatalog = "test"
            };

            var db = SqlDatabase.CreateBuilder()
                .WithConnectionString(cs.ToString())
                .AddMigrations(migrationsPath)
                .Build();

            await db.Drop();

            await db.Create();

            // Act
            var count = await db.MigrateToLatest();

            // Assert
            count.Should().Be(4);

            var migrations = await db.GetAppliedMigrations(db.GetConnection());

            // 4 scripts + database create
            migrations.Count().Should().Be(5);
        }

        [Fact]
        public async void ShouldApplyMigrationToVersion()
        {
            // Arrange
            var migrationsPath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts");

            var cs = new SqlConnectionStringBuilder(_fixture.DbContainer.ConnectionString)
            {
                InitialCatalog = "test"
            };

            var db = SqlDatabase.CreateBuilder()
                .WithConnectionString(cs.ToString())
                .AddMigrations(migrationsPath)
                .Build();

            await db.Drop();

            await db.Create();

            await db.MigrateTo(new Version(1, 0));

            // Act
            var count = await db.MigrateToLatest();

            // Assert
            count.Should().Be(3);

            var migrations = await db.GetAppliedMigrations(db.GetConnection());

            // 4 scripts + database create
            migrations.Count().Should().Be(5);
        }

        [Fact]
        public async void ShouldNotApplySkippedMigrations()
        {
            // Arrange

            // Apply migrations, but skip "1.1 one-one.sql"
            var migrationsPath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts - One Missing");

            var cs = new SqlConnectionStringBuilder(_fixture.DbContainer.ConnectionString)
            {
                InitialCatalog = "test"
            };

            var db = SqlDatabase.CreateBuilder()
                .WithConnectionString(cs.ToString())
                .AddMigrations(migrationsPath)
                .Build();

            await db.Drop();

            await db.Create();

            await db.MigrateToLatest();

            // Now set up for migrating again, but with the missing script
            migrationsPath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts");

            db = SqlDatabase.CreateBuilder()
                .WithConnectionString(cs.ToString())
                .AddMigrations(migrationsPath)
                .Build();

            // Act
            var ex = await Assert.ThrowsAsync<MigrationsNotAppliedInOrderException>(db.MigrateToLatest);

            // Assert
            ex.Should().NotBeNull();
        }
    }
}
