using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;

namespace WillSoss.DbDeploy.Cli
{
    internal class DropCommand : ICliCommand
    {
        private DatabaseBuilder _builder;
        private readonly string? _connectionString;
        private readonly bool _unsafe;
        private readonly ILogger _logger;

        public DropCommand(DatabaseBuilder builder, string? connectionString, bool @unsafe, ILogger<DropCommand> logger)
        {
            _builder = builder;
            _connectionString = connectionString;
            _unsafe = @unsafe;
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
            
            if (_unsafe)
                _logger.LogWarning("UNSAFE IS ON: Production keyword protections are disabled for destructive actions.");

            _logger.LogInformation("Dropping database {0} on {1}.", db.GetDatabaseName(), db.GetServerName());

            await db.Drop(_unsafe);

            _logger.LogInformation("Database {0} dropped on {1}.", db.GetDatabaseName(), db.GetServerName());
        }

        internal static Command Create(IServiceCollection services)
        {
            var command = new Command("drop", "Drops the database if it exists."); ;

            command.AddOption(CliOptions.UnsafeOption);

            command.SetHandler((cs, @unsafe) => services.AddTransient<ICliCommand>(s => new DropCommand(
                s.GetRequiredService<DatabaseBuilder>(),
                cs,
                @unsafe,
                s.GetRequiredService<ILogger<DropCommand>>()
                )), CliOptions.ConnectionStringOption, CliOptions.UnsafeOption);

            return command;
        }
    }
}
