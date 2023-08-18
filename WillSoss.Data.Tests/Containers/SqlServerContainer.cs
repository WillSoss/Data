using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

namespace WillSoss.Data.Tests.Containers
{
    public class SqlServerContainer : DockerContainer
    {
        readonly SqlServerConfiguration _config;

        public string ConnectionString => $"server=localhost,{GetMappedPublicPort(1433)};database=master;uid=sa;pwd={_config.Password};trustservercertificate=true;";

        internal SqlServerContainer(SqlServerConfiguration config)
            : base(config, TestcontainersSettings.Logger) 
        {
            _config = config;
        }
    }
}
