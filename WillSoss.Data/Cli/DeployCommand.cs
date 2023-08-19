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

            if (_version is null)
                _logger.LogInformation("Migrating database {0} on {1} to latest.", db.GetDatabaseName(), db.GetServerName());

            else
                _logger.LogInformation("Migrating database {0} on {1} to version {2}.", db.GetDatabaseName(), db.GetServerName(), _version);

            await db.MigrateTo(_version);

            _logger.LogInformation("Deployment complete for database {0} on {1}.", db.GetDatabaseName(), db.GetServerName());
        }

        internal static Command Create(IServiceCollection services)
        {
            var deploy = new Command("deploy", "Creates the database if it does not exist, then migrates to latest."); ;

            var connectionStringOption = new Option<string?>(new[] { "--connectionString", "-c" },
                description: "Connection string of the database to modify. Optional when a default is supplied by the application.");

            var versionOption = new Option<Version?>(new[] { "--version", "-v" },
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

            deploy.AddOption(connectionStringOption);
            deploy.AddOption(versionOption);
            deploy.AddOption(dropOption);

            deploy.SetHandler((cs, version, drop) => services.AddTransient<CliCommand>(s => new DeployCommand(
                s.GetRequiredService<DatabaseBuilder>(),
                cs,
                version,
                drop,
                s.GetRequiredService<ILogger<DeployCommand>>()
                )), connectionStringOption, versionOption, dropOption);

            return deploy;
        }
    }
}
