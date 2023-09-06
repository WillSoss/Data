using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;

namespace WillSoss.DbDeploy.Cli
{
    internal class StatusCommand : RootCommand, ICliCommand
    {
        private DatabaseBuilder _builder;
        private readonly string? _connectionString;
        private readonly ILogger _logger;

        public StatusCommand(DatabaseBuilder builder, string? connectionString, ILogger<StatusCommand> logger)
        {
            _builder = builder;
            _connectionString = connectionString;
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

            var unapplied = await db.GetUnappliedMigrations();

            Console.WriteLine($"Database {db.GetDatabaseName()} on server {db.GetServerName()} is at version {at!.Version} ({at} - {at.Description}).");
            Console.WriteLine();

            await ConsoleMessages.WriteDatabaseInfo(db);

            Console.WriteLine();

            if (unapplied?.Count() == 0)
            {
                Console.WriteLine("There are no unapplied migrations. The database is up to date.");
            }
            else
            {
                Console.WriteLine(" Migrations not applied to database:");
                Console.WriteLine();

                foreach (var v in unapplied!.GroupBy(m => m.Version))
                {
                    Console.WriteLine($" Version {v.Key}");
                    Console.WriteLine();

                    foreach (var p in v.GroupBy(v => v.Phase))
                    {
                        Console.WriteLine($"   {p.Key}-deployment scripts");

                        foreach (var script in p)
                        {
                            Console.Write($"     Script ");
                            ConsoleMessages.WriteColorLine(script.FileName, ConsoleColor.Blue);
                        }

                        Console.WriteLine();
                    }
                }
            }
        }



        private void WriteCaution(string text)
        {
            var background = Console.BackgroundColor;
            var foreground = Console.ForegroundColor;

            Console.BackgroundColor = ConsoleColor.DarkYellow;
            Console.ForegroundColor = ConsoleColor.White;

            Console.Write($" !! CAUTION !! {text} ");

            Console.BackgroundColor = background;
            Console.ForegroundColor = foreground;

            Console.WriteLine();
            Console.WriteLine();
        }

    }
}
