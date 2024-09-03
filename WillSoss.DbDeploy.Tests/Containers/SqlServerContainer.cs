using DotNet.Testcontainers.Containers;

namespace WillSoss.DbDeploy.Tests.Containers
{
    public class SqlServerContainer : DockerContainer
    {
        readonly SqlServerConfiguration _config;

        public string ConnectionString => $"server=localhost,{GetMappedPublicPort(1433)};database=master;uid=sa;pwd={_config.Password};trustservercertificate=true;";

        internal SqlServerContainer(SqlServerConfiguration config)
            : base(config) 
        {
            _config = config;
        }
    }
}
