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
            var connectionString = "";
            var migrationsPath = "";

            var db = AzureSqlDatabase
                .ConnectTo(connectionString)
                .AddMigrations(migrationsPath)
                .WithCommandTimeout(90)
                .AddProductionKeywords("prod")
                .Build();

            await db.MigrateToLatest();

            await db.Drop();


                


        }
    }
}
