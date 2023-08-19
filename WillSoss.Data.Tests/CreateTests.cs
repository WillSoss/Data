using Dapper;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using WillSoss.Data.Sql;

namespace WillSoss.Data.Tests
{
    public class CreateTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly IntegrationTestFixture _fixture;

        public CreateTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task ShouldCreateMigrationsSchema()
        {
            // Arrange

            var cs = new SqlConnectionStringBuilder(_fixture.DbContainer.ConnectionString);
            cs.InitialCatalog = "test";

            var db = SqlDatabase.CreateBuilder()
                .WithConnectionString(cs.ToString())
                .Build();

            // Act
            await db.Create();

            // Assert
            var migrations = await db.GetConnection().QueryAsync<Migration>("select * from cfg.migration_detail");

            migrations.Count().Should().Be(1);
            migrations.Single().Version.Should().Be(new Version(0, 0, 0, 0));
            migrations.Single().Description.Should().Be("Database Created");
        }
    }
}
