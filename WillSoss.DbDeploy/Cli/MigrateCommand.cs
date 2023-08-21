using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;

namespace WillSoss.DbDeploy.Cli
{
    internal class MigrateCommand : ICliCommand
    {
        private DatabaseBuilder _builder;
        private readonly string? _connectionString;
        private readonly Version? _version;
        private readonly ILogger _logger;

        public MigrateCommand(DatabaseBuilder builder, string? connectionString, Version? version, ILogger<MigrateCommand> logger)
        {
            _builder = builder;
            _connectionString = connectionString;
            _version = version;
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

            if (_version is null)
                _logger.LogInformation("Migrating database {0} on {1} to latest.", db.GetDatabaseName(), db.GetServerName());

            else
                _logger.LogInformation("Migrating database {0} on {1} to version {2}.", db.GetDatabaseName(), db.GetServerName(), _version);

            await db.MigrateTo(_version);

            _logger.LogInformation("Migration complete for database {0} on {1}.", db.GetDatabaseName(), db.GetServerName());
        }

        internal static Command Create(IServiceCollection services)
        {
            var command = new Command("migrate", "Migrates to the specified version, or latest if no version is specified."); ;

            command.AddOption(CliOptions.ConnectionStringOption);
            command.AddOption(CliOptions.VersionOption);

            command.SetHandler((cs, version) => services.AddTransient<ICliCommand>(s => new MigrateCommand(
                s.GetRequiredService<DatabaseBuilder>(),
                cs,
                version,
                s.GetRequiredService<ILogger<MigrateCommand>>()
                )), CliOptions.ConnectionStringOption, CliOptions.VersionOption);

            return command;
        }
    }
}
