using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;

namespace WillSoss.DbDeploy.Tests.Containers
{
    public class SqlServerConfiguration : ContainerConfiguration
    {
        public string? Password { get; }

        public SqlServerConfiguration(string? password = null) => Password = password;

        public SqlServerConfiguration(IResourceConfiguration<CreateContainerParameters> config)
            : base(config) { }

        public SqlServerConfiguration(IContainerConfiguration config)
            : base(config) { }

        public SqlServerConfiguration(SqlServerConfiguration config)
            : this(new SqlServerConfiguration(), config) { }

        public SqlServerConfiguration(SqlServerConfiguration oldValue, SqlServerConfiguration newValue)
            : base(oldValue, newValue) 
        {
            Password = BuildConfiguration.Combine(oldValue.Password, newValue.Password);
        }
    }
}
