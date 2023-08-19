using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;

namespace WillSoss.Data.Cli
{
    internal class CreateCommand : CliCommand
    {
        private DatabaseBuilder _builder;
        private readonly string? _connectionString;
        private readonly bool _drop;
        private readonly ILogger _logger;

        public CreateCommand(DatabaseBuilder builder, string? connectionString, bool drop, ILogger<CreateCommand> logger)
        {
            _builder = builder;
            _connectionString = connectionString;
            _drop = drop;
            _logger = logger;
        }

        internal override async Task RunAsync(CancellationToken cancel)
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

            _logger.LogInformation("Database {0} created on {1}.", db.GetDatabaseName(), db.GetServerName());
        }

        internal static Command Create(IServiceCollection services)
        {
            var deploy = new Command("create", "Creates the database if it does not exist."); ;

            deploy.AddOption(ConnectionStringOption);
            deploy.AddOption(DropOption);

            deploy.SetHandler((cs, drop) => services.AddTransient<CliCommand>(s => new CreateCommand(
                s.GetRequiredService<DatabaseBuilder>(),
                cs,
                drop,
                s.GetRequiredService<ILogger<CreateCommand>>()
                )), ConnectionStringOption, DropOption);

            return deploy;
        }
    }
}
