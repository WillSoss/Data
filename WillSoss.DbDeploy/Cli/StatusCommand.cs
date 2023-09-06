using System.CommandLine;

namespace WillSoss.DbDeploy.Cli
{
    internal class StatusCommand : ICliCommand
    {
        private DatabaseBuilder _builder;
        private readonly string? _connectionString;

        public StatusCommand(DatabaseBuilder builder, string? connectionString)
        {
            _builder = builder;
            _connectionString = connectionString;
        }

        async Task ICliCommand.RunAsync(CancellationToken cancel)
        {
            if (!string.IsNullOrWhiteSpace(_connectionString))
                _builder = _builder.WithConnectionString(_connectionString);

            if (string.IsNullOrWhiteSpace(_builder.ConnectionString))
            {
                ConsoleMessages.WriteError("Connection string is required. Configure the connection string in the app or use --connectionstring <connectionstring>.");
                return;
            }

            var db = _builder.Build();

            var unapplied = await db.GetUnappliedMigrations();

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
    }
}
