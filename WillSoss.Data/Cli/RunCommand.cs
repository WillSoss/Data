 using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;

namespace WillSoss.Data.Cli
{
    internal class RunCommand : ICliCommand
    {
        private DatabaseBuilder _builder;
        private readonly string? _connectionString;
        private readonly string? _action;
        private readonly ILogger _logger;

        public RunCommand(DatabaseBuilder builder, string? connectionString, string? action, ILogger<RunCommand> logger)
        {
            _builder = builder;
            _connectionString = connectionString;
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

            var scriptKey = db.NamedScripts.Keys.FirstOrDefault(k => k.Equals(_action, StringComparison.InvariantCultureIgnoreCase));
            var actionKey = db.Actions.Keys.FirstOrDefault(k => k.Equals(_action, StringComparison.InvariantCultureIgnoreCase));

            if (scriptKey is not null) 
            {
                var script = db.NamedScripts[scriptKey!];

                _logger.LogInformation("Running script {0} on database {1} on {2}.", script.FileName, db.GetDatabaseName(), db.GetServerName());

                await db.ExecuteScriptAsync(script, db.GetConnection());

                _logger.LogInformation("Script run complete on database {0} on {1}.", db.GetDatabaseName(), db.GetServerName());
            }
            else if (actionKey is not null)
            {
                var action = db.Actions[actionKey!];

                _logger.LogInformation("Running action {0} on database {1} on {2}.", _action, db.GetDatabaseName(), db.GetServerName());

                await action(db);

                _logger.LogInformation("Action complete on database {0} on {1}.", db.GetDatabaseName(), db.GetServerName());
            }
            else
            {
                _logger.LogError("A script or action named {0} could not be found.", _action);
            }
        }

        internal static Command Create(IServiceCollection services)
        {
            var command = new Command("run", "Executes a script or action."); ;

            var arg = new Argument<string?>("script", "The script or action to run.")
            {
                Arity = ArgumentArity.ExactlyOne
            };
            
            command.AddArgument(arg);

            command.SetHandler((cs, action) => services.AddTransient<ICliCommand>(s => new RunCommand(
                s.GetRequiredService<DatabaseBuilder>(),
                cs,
                action,
                s.GetRequiredService<ILogger<RunCommand>>()
                )), CliOptions.ConnectionStringOption, arg);

            return command;
        }
    }
}
