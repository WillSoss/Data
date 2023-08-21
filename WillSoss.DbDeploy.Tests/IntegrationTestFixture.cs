using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Networks;
using System.Diagnostics;
using WillSoss.DbDeploy.Tests.Containers;

namespace WillSoss.DbDeploy.Tests
{
    public class IntegrationTestFixture : IAsyncLifetime
    {
        public const string DatabaseName = "TestDb";
        public const string Password = "Password!";
        public readonly INetwork Network;
        public readonly SqlServerContainer DbContainer;

        public IntegrationTestFixture()
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
            await WaitForContainer(TimeSpan.FromSeconds(10));
        }

        async Task WaitForContainer(TimeSpan timeout)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (true)
            {
                if (DbContainer.Health == DotNet.Testcontainers.Containers.TestcontainersHealthStatus.Healthy)
                    return;

                if (stopwatch.Elapsed > timeout)
                    return;

                await Task.Delay(200);
            }
        }

        public async Task DisposeAsync()
        {
            await DbContainer.StopAsync();
        }
    }
}
