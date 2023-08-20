using Dapper;
using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.Data.SqlClient;

namespace WillSoss.Data.Tests.Containers
{
    public class SqlServerContainerBuilder : ContainerBuilder<SqlServerContainerBuilder, SqlServerContainer, SqlServerConfiguration>
    {
        protected override SqlServerConfiguration DockerResourceConfiguration { get; } = new SqlServerConfiguration();

        public SqlServerContainerBuilder()
            : base(new SqlServerConfiguration()) => DockerResourceConfiguration = Init().DockerResourceConfiguration;

        public SqlServerContainerBuilder(SqlServerConfiguration config)
            : base(config) => DockerResourceConfiguration = config;

        protected override SqlServerContainerBuilder Init()
        {
            return base.Init()
                .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                .WithPortBinding(1433, true)
                .WithEnvironment("ACCEPT_EULA", "Y")
                .WithPassword("Password!")
                .WithWaitStrategy(Wait.ForUnixContainer().AddCustomWaitStrategy(new WaitUntilSqlIsReady()));
        }

        class WaitUntilSqlIsReady : IWaitUntil
        {
            public async Task<bool> UntilAsync(IContainer container)
            {
                try
                {
                    var sql = (SqlServerContainer)container;

                    using var db = new SqlConnection(sql.ConnectionString);
                    db.Open();

                    using var cmd = new SqlCommand("select 1", db);

                    return (await db.QueryFirstOrDefaultAsync<int?>("select 1")) == 1;
                }
                catch
                {
                    return false;
                }
            }
        }

        public SqlServerContainerBuilder WithPassword(string password) =>
            Merge(DockerResourceConfiguration, new SqlServerConfiguration(password: password))
                .WithEnvironment("MSSQL_SA_PASSWORD", password);

        public override SqlServerContainer Build() => new SqlServerContainer(DockerResourceConfiguration);

        protected override SqlServerContainerBuilder Clone(IContainerConfiguration resourceConfiguration) =>
            Merge(new SqlServerConfiguration(resourceConfiguration), DockerResourceConfiguration ?? new SqlServerConfiguration());

        protected override SqlServerContainerBuilder Clone(IResourceConfiguration<CreateContainerParameters> resourceConfiguration) =>
            Merge(new SqlServerConfiguration(resourceConfiguration), DockerResourceConfiguration ?? new SqlServerConfiguration());

        protected override SqlServerContainerBuilder Merge(SqlServerConfiguration oldValue, SqlServerConfiguration newValue) =>
            new SqlServerContainerBuilder(new SqlServerConfiguration(oldValue, newValue));
    }
}
