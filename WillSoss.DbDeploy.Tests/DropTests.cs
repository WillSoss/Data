using FluentAssertions;
using Microsoft.Data.SqlClient;
using WillSoss.DbDeploy.Sql;

namespace WillSoss.DbDeploy.Tests
{
    public class DropTests : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _fixture;

        public DropTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task WithProdInDbName_ShouldNotDropDatabase()
        {
            // Arrange

            var cs = new SqlConnectionStringBuilder(_fixture.DbContainer.ConnectionString);
            cs.InitialCatalog = "test-prod";

            var db = SqlDatabase.CreateBuilder()
                .WithConnectionString(cs.ToString())
                .Build();

            // Act
            var ex = await db.Invoking(db => db.Drop())
                .Should().ThrowAsync<InvalidOperationException>();

            // Assert
        }
    }
}
