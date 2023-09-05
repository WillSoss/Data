using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;

namespace WillSoss.DbDeploy.Cli
{
    internal class StatusCommand : RootCommand, ICliCommand
    {
        private DatabaseBuilder _builder;
        private readonly string? _connectionString;
        private readonly Version? _version;
        private readonly ILogger _logger;

        public StatusCommand(DatabaseBuilder builder, string? connectionString, Version? version, ILogger<StatusCommand> logger)
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

            var at = (await db.GetAppliedMigrations()).LastOrDefault();

            Console.WriteLine($"Database {db.GetDatabaseName()} on server {db.GetServerName()} is at version {at!.Version} ({at} - {at.Description}).");
            Console.WriteLine();

            var unapplied = await db.GetUnappliedMigrations();

            if (unapplied.Count() == 0)
            {
                Console.WriteLine("There are no unapplied migrations. The database is up to date.");
            }
            else
            {
                Console.WriteLine("Unapplied migrations:");
                foreach (var script in unapplied)
                    Console.WriteLine(script);
            }

            //if (_drop)
            //{
            //    _logger.LogInformation("Dropping database {0} on {1}.", db.GetDatabaseName(), db.GetServerName());

            //    await db.Drop();
            //}

            //_logger.LogInformation("Creating database {0} on {1}.", db.GetDatabaseName(), db.GetServerName());

            //await db.Create();

            //if (_version is null)
            //    _logger.LogInformation("Migrating database {0} on {1} to latest.", db.GetDatabaseName(), db.GetServerName());

            //else
            //    _logger.LogInformation("Migrating database {0} on {1} to version {2}.", db.GetDatabaseName(), db.GetServerName(), _version);

            //await db.MigrateTo(_version);

            //_logger.LogInformation("Deployment complete for database {0} on {1}.", db.GetDatabaseName(), db.GetServerName());
        }

        internal static RootCommand Create(IServiceCollection services)
        {
            var command = new RootCommand("Displays the migration status of the database."); ;

            command.AddOption(CliOptions.ConnectionStringOption);
            command.AddOption(CliOptions.VersionOption);

            command.SetHandler((cs, version) => services.AddTransient<ICliCommand>(s => new StatusCommand(
                s.GetRequiredService<DatabaseBuilder>(),
                cs,
                version,
                s.GetRequiredService<ILogger<StatusCommand>>()
                )), CliOptions.ConnectionStringOption, CliOptions.VersionOption);

            return command;
        }
    }
}
