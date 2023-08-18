using FluentAssertions;
using Microsoft.Data.SqlClient;
using WillSoss.Data.Sql;

namespace WillSoss.Data.Tests
{
    public class MigrateTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly IntegrationTestFixture _fixture;

        public MigrateTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task ShouldLoadMigrations()
        {
            // Arrange
            var migrationsPath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts");

            var cs = new SqlConnectionStringBuilder(_fixture.DbContainer.ConnectionString);
            cs.InitialCatalog = "test";

            // Act
            var db = SqlDatabase
                .ConnectTo(cs.ToString())
                .AddMigrations(migrationsPath)
                .Build();

            // Assert
            db.Migrations.Count().Should().Be(4);
        }
    }
}
