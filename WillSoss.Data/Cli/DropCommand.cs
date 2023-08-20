using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;

namespace WillSoss.Data.Cli
{
    internal class DropCommand : CliCommand
    {
        internal static Option<bool> DropProductionOption = new Option<bool>(new[] { "--dropproduction" }, "Use with extreme caution. Drops a production database by overriding the production keyword protections.");

        private DatabaseBuilder _builder;
        private readonly string? _connectionString;
        private readonly bool _dropProduction;
        private readonly ILogger _logger;

        public DropCommand(DatabaseBuilder builder, string? connectionString, bool dropProduction, ILogger<DropCommand> logger)
        {
            _builder = builder;
            _connectionString = connectionString;
            _dropProduction = dropProduction;
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

            _logger.LogInformation("Dropping database {0} on {1}.", db.GetDatabaseName(), db.GetServerName());

            await db.Drop(_dropProduction);

            _logger.LogInformation("Database {0} dropped on {1}.", db.GetDatabaseName(), db.GetServerName());
        }

        internal static Command Create(IServiceCollection services)
        {
            var command = new Command("drop", "Drops the database if it exists."); ;

            command.AddOption(ConnectionStringOption);
            command.AddOption(DropOption);

            command.SetHandler((cs, drop) => services.AddTransient<CliCommand>(s => new DropCommand(
                s.GetRequiredService<DatabaseBuilder>(),
                cs,
                drop,
                s.GetRequiredService<ILogger<DropCommand>>()
                )), ConnectionStringOption, DropProductionOption);

            return command;
        }
    }
}
