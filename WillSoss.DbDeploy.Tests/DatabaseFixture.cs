using Dapper;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Networks;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using WillSoss.DbDeploy.Sql;
using WillSoss.DbDeploy.Tests.Containers;

namespace WillSoss.DbDeploy.Tests
{
    public class DatabaseFixture : IAsyncLifetime
    {
        public const string DatabaseName = "TestDb";
        public const string Password = "Password!";
        public readonly INetwork Network;
        public readonly SqlServerContainer DbContainer;

        public DatabaseFixture()
        {
            Network = new NetworkBuilder().Build();

            DbContainer = new SqlServerContainerBuilder()
                .WithNetwork(Network)
                .WithPassword(Password)
                .Build();
        }

        public async Task InitializeAsync()
        {
            await DbContainer.StartAsync();
            await WaitForContainer(TimeSpan.FromSeconds(60));
        }

        async Task WaitForContainer(TimeSpan timeout)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (true)
            {
                if (await IsSqlServerReady())
                    return;

                if (stopwatch.Elapsed > timeout)
                    throw new TimeoutException("Timeout occured while waiting for the container to start.");

                await Task.Delay(250);
            }
        }

        public async Task<bool> IsSqlServerReady()
        {
            try
            {
                using var db = new SqlConnection(DbContainer.ConnectionString);
                db.Open();

                using var cmd = new SqlCommand("select 1", db);

                return (await db.QueryFirstOrDefaultAsync<int?>("select 1")) == 1;
            }
            catch
            {
                return false;
            } 
        }

        public async Task DisposeAsync()
        {
            await DbContainer.StopAsync();
        }
    }
}