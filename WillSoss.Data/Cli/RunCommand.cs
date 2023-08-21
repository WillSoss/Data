 using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;

namespace WillSoss.Data.Cli
{
    internal class RunCommand : ICliCommand
    {
        private DatabaseBuilder _builder;
        private readonly string? _connectionString;
        private readonly string? _script;
        private readonly string? _action;
        private readonly ILogger _logger;

        public RunCommand(DatabaseBuilder builder, string? connectionString, string? script, string? action, ILogger<RunCommand> logger)
        {
            _builder = builder;
            _connectionString = connectionString;
            _script = script;
            _action = action;
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

            if (!string.IsNullOrEmpty(_script)) 
            {
                var key = db.NamedScripts.Keys.FirstOrDefault(k => k.Equals(_script, StringComparison.InvariantCultureIgnoreCase));

                if (key is null)
                    _logger.LogError("Script {0} not found", _script);

                var script = db.NamedScripts[key!];

                _logger.LogInformation("Running script {0} on database {1} on {2}.", script.FileName, db.GetDatabaseName(), db.GetServerName());

                await db.ExecuteScriptAsync(script, db.GetConnection());

                _logger.LogInformation("Script run complete on database {0} on {1}.", db.GetDatabaseName(), db.GetServerName());
            }
            else if (!string.IsNullOrWhiteSpace(_action))
            {
                var key = db.Actions.Keys.FirstOrDefault(k => k.Equals(_action, StringComparison.InvariantCultureIgnoreCase));

                if (key is null)
                    _logger.LogError("Action {0} not found", _action);

                var action = db.Actions[key!];

                _logger.LogInformation("Running action {0} on database {1} on {2}.", _action, db.GetDatabaseName(), db.GetServerName());

                await action(db);

                _logger.LogInformation("Action complete on database {0} on {1}.", db.GetDatabaseName(), db.GetServerName());
            }
            else
            {
                _logger.LogError("A script or action name must be provided with the run command.");
            }
        }

        internal static Command Create(IServiceCollection services)
        {
            var command = new Command("run", "Executes a script or action."); ;

            command.AddOption(CliOptions.ScriptOption);
            command.AddOption(CliOptions.ActionOption);

            command.SetHandler((cs, script, action) => services.AddTransient<ICliCommand>(s => new RunCommand(
                s.GetRequiredService<DatabaseBuilder>(),
                cs,
                script,
                action,
                s.GetRequiredService<ILogger<RunCommand>>()
                )), CliOptions.ConnectionStringOption, CliOptions.ScriptOption, CliOptions.ActionOption);

            return command;
        }
    }
}
