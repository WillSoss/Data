 using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;

namespace WillSoss.Data.Cli
{
    internal class ExecCommand : CliCommand
    {
        private DatabaseBuilder _builder;
        private readonly string? _connectionString;
        private readonly ILogger _logger;

        public ExecCommand(DatabaseBuilder builder, string? connectionString, ILogger<DeployCommand> logger)
        {
            _builder = builder;
            _connectionString = connectionString;
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

            //_logger.LogInformation("Migrating database {0} on {1} to version {2}.", db.GetDatabaseName(), db.GetServerName(), _version);

            //await db.MigrateTo(_version);

            _logger.LogInformation("Deployment complete for database {0} on {1}.", db.GetDatabaseName(), db.GetServerName());
        }

        internal static Command Create(IServiceCollection services)
        {
            var command = new Command("exec", "Executes named scripts against the database."); ;

            command.AddOption(ConnectionStringOption);
            command.AddOption(VersionOption);
            command.AddOption(DropOption);

            //command.SetHandler((cs) => services.AddTransient<CliCommand>(s => new ExecCommand(
            //    s.GetRequiredService<DatabaseBuilder>(),
            //    cs,
            //    s.GetRequiredService<ILogger<ExecCommand>>()
            //    )), ConnectionStringOption);

            return command;
        }
    }
}
