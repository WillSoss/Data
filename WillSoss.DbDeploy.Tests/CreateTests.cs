using Dapper;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using WillSoss.DbDeploy.Sql;

namespace WillSoss.DbDeploy.Tests
{
    public class CreateTests : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _fixture;

        public CreateTests(DatabaseFixture fixture)
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
            migrations.Should().BeEquivalentTo(new[]
            {
                new Migration
                {
                    Version = new Version(0,0),
                    Phase = MigrationPhase.Pre,
                    Number = 0,
                    Description = "Database Created"
                }
            });
        }

        [Fact]
        public async Task WithExistingDb_AddsMigrationSchema()
        {
            // Arrange

            var master = new SqlConnectionStringBuilder(_fixture.DbContainer.ConnectionString);
            master.InitialCatalog = "master";

            var cs = new SqlConnectionStringBuilder(_fixture.DbContainer.ConnectionString);
            cs.InitialCatalog = "test";

            var db = SqlDatabase.CreateBuilder()
                .WithConnectionString(cs.ToString())
                .Build();

            await db.ExecuteScriptAsync(SqlDatabase.OnPremiseCreateScript, new SqlConnection(master.ToString()), replacementTokens: new()
            {
                { "database", "test" }
            });

            // Act
            await db.Create();

            // Assert
            var migrations = await db.GetConnection().QueryAsync<Migration>("select * from cfg.migration_detail");

            migrations.Count().Should().Be(1);
            migrations.Should().BeEquivalentTo(new[]
            {
                new Migration
                {
                    Version = new Version(0,0),
                    Phase = MigrationPhase.Pre,
                    Number = 0,
                    Description = "Database Created"
                }
            });
        }
    }
}
