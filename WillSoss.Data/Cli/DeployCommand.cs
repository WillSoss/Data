using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WillSoss.Data.Cli
{
    internal class DeployCommand : CliCommand
    {
        private readonly Database _database;
        private readonly Version? _version;
        private readonly bool _drop;
        private readonly ILogger _logger;

        public DeployCommand(Database database, Version? version, bool drop, ILogger<DeployCommand> logger)
        {
            _database = database;
            _version = version;
            _drop = drop;
            _logger = logger;
        }

        internal override async Task RunAsync(CancellationToken cancel)
        {
            if (_drop)
            {
                _logger.LogInformation("Dropping database {0} on {1}.", _database.GetDatabaseName(), _database.GetServerName());

                await _database.Drop();
            }

            _logger.LogInformation("Creating database {0} on {1}.", _database.GetDatabaseName(), _database.GetServerName());

            await _database.Create();

            if (_version is null)
                _logger.LogInformation("Migrating database {0} on {1} to latest.", _database.GetDatabaseName(), _database.GetServerName());

            else
                _logger.LogInformation("Migrating database {0} on {1} to version {2}.", _database.GetDatabaseName(), _database.GetServerName(), _version);

            await _database.MigrateTo(_version);

            _logger.LogInformation("Deployment complete for database {0} on {1} to version {2}.", _database.GetDatabaseName(), _database.GetServerName());
        }

        internal static Command Create(IServiceCollection services)
        {
            var deploy = new Command("deploy", "Creates the database if it does not exist, then migrates to latest."); ;

            var versionOption = new Option<Version>(new[] { "--version", "-v" },
                description: "Optional. Migrates to the specified version instead of latest.",
                parseArgument: result =>
                {
                    Version? version;
                    if (!Version.TryParse(result.Tokens[0].Value, out version))
                        result.ErrorMessage = "Version must be in the format #[.#[.#[.#]]]";

                    return version!.FillZeros();
                })
            {
                Arity = ArgumentArity.ExactlyOne
            };

            var dropOption = new Option<bool>(new[] { "--drop", "-d" }, "Optional. Drops the database if it exists, then recreates and migrates.");

            deploy.AddOption(versionOption);
            deploy.AddOption(dropOption);

            deploy.SetHandler<Version?, bool>((version, drop) => services.AddTransient<CliCommand>(s => new DeployCommand(
                s.GetRequiredService<Database>(),
                version,
                drop,
                s.GetRequiredService<ILogger<DeployCommand>>()
                )), versionOption, dropOption);

            return deploy;
        }
    }
}
