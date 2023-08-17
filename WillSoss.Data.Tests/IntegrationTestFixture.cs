using System.Security.Cryptography;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;

namespace WillSoss.Data.Tests
{
    public class IntegrationTestFixture : IAsyncLifetime
    {
        public const string DatabaseName = "TestDb";
        public readonly MsSqlTestcontainer DbContainer;

        public IntegrationTestFixture()
        {
            var databaseServerContainerConfig = new MsSqlTestcontainerConfiguration();
            databaseServerContainerConfig.Database = DatabaseName;
            databaseServerContainerConfig.Password = Convert.ToBase64String(RandomNumberGenerator.GetBytes(12));
            databaseServerContainerConfig.Environments.Add("MSSQL_PID", "Express");

            DbContainer = new TestcontainersBuilder<MsSqlTestcontainer>()
                 .WithDatabase(databaseServerContainerConfig)
                 .Build();
        }

        public async Task InitializeAsync()
        {
            await DbContainer.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await DbContainer.StopAsync();
        }
    }
}
