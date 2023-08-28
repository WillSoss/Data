using Microsoft.Data.SqlClient;
using WillSoss.DbDeploy.Sql;

namespace WillSoss.DbDeploy.Tests
{
    public class DatabaseTest : IAsyncLifetime
    {
        protected readonly DatabaseFixture _fixture;
        private readonly string _databaseName;

        protected string ConnectionString
        {
            get
            {
                return new SqlConnectionStringBuilder(_fixture.DbContainer.ConnectionString)
                {
                    InitialCatalog = _databaseName
                }.ToString();
            }
        }

        public DatabaseTest(DatabaseFixture fixture, string databaseName)
        {
            _fixture = fixture;
            _databaseName = databaseName;
        }

        public Task InitializeAsync() => BeforeEach();

        public virtual async Task BeforeEach()
        {
            var db = SqlDatabase.CreateBuilder()
                .WithConnectionString(ConnectionString)
                .Build();

            await db.Drop();
        }

        public Task DisposeAsync() => Task.CompletedTask;
    }
}
