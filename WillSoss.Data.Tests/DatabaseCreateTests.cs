using Dapper;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WillSoss.Data.Sql;

namespace WillSoss.Data.Tests
{
    public class MigrationSetupTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly IntegrationTestFixture _fixture;

        public MigrationSetupTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task ShouldCreateMigrationsSchema()
        {
            // Arrange

            var cs = new SqlConnectionStringBuilder(_fixture.DbContainer.ConnectionString);
            cs.InitialCatalog = "test";

            var db = SqlDatabase
                .ConnectTo(cs.ToString())
                .Build();

            // Act
            await db.Create();

            // Assert
            var result = await db.GetConnection().QueryAsync("select * from cfg.migration");

            var migrations = await db.GetConnection().QueryAsync<Migration>("select * from cfg.migration_detail");

            migrations.Count().Should().Be(1);
            migrations.Single().Version.Should().Be(new Version(0, 0, 0, 0));
            migrations.Single().Description.Should().Be("Database Created");
        }
    }
}
