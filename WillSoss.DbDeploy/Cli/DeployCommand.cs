using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;

namespace WillSoss.DbDeploy.Cli
{
    internal class DeployCommand : RootCommand, ICliCommand
    {
        private DatabaseBuilder _builder;
        private readonly string? _connectionString;
        private readonly Version? _version;
        private readonly bool _drop;
        private readonly ILogger _logger;

        public DeployCommand(DatabaseBuilder builder, string? connectionString, Version? version, bool drop, ILogger<DeployCommand> logger)
        {
            _builder = builder;
            _connectionString = connectionString;
            _version = version;
            _drop = drop;
            _logger = logger;
        }

        async Task ICliCommand.RunAsync(CancellationToken cancel)
        {
            if (!string.IsNullOrWhiteSpace(_connectionString))
                _builder = _builder.WithConnectionString(_connectionString);

            if (string.IsNullOrWhiteSpace(_builder.ConnectionString))
            {
                _logger.LogError("Connection string is required. Configure the connection string in the app or use --connectionstring <connectionstring>.");
                return;
            }

            var db = _builder.Build();

            if (_drop)
            {
                _logger.LogInformation("Dropping database {0} on {1}.", db.GetDatabaseName(), db.GetServerName());

                await db.Drop();
            }

            _logger.LogInformation("Creating database {0} on {1}.", db.GetDatabaseName(), db.GetServerName());

            await db.Create();

            if (_version is null)
                _logger.LogInformation("Migrating database {0} on {1} to latest.", db.GetDatabaseName(), db.GetServerName());

            else
                _logger.LogInformation("Migrating database {0} on {1} to version {2}.", db.GetDatabaseName(), db.GetServerName(), _version);

            await db.MigrateTo(_version);

            _logger.LogInformation("Deployment complete for database {0} on {1}.", db.GetDatabaseName(), db.GetServerName());
        }

        internal static RootCommand Create(IServiceCollection services)
        {
            var command = new RootCommand("Creates the database if it does not exist, then migrates to latest."); ;

            command.AddOption(CliOptions.ConnectionStringOption);
            command.AddOption(CliOptions.VersionOption);
            command.AddOption(CliOptions.DropOption);

            command.SetHandler((cs, version, drop) => services.AddTransient<ICliCommand>(s => new DeployCommand(
                s.GetRequiredService<DatabaseBuilder>(),
                cs,
                version,
                drop,
                s.GetRequiredService<ILogger<DeployCommand>>()
                )), CliOptions.ConnectionStringOption, CliOptions.VersionOption, CliOptions.DropOption);

            return command;
        }
    }
}
