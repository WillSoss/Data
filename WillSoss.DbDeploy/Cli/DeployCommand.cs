namespace WillSoss.DbDeploy.Cli
{
    internal class DeployCommand : ICliCommand
    {
        private DatabaseBuilder _builder;
        private readonly string? _connectionString;
        private readonly Version? _version;
        private readonly bool _drop;
        private readonly bool _unsafe;
        private readonly bool _create;
        private readonly bool _migrate;
        private readonly bool _applyMissing;
        private readonly bool _pre;
        private readonly bool _post;

        public DeployCommand(DatabaseBuilder builder, string? connectionString, Version? version, bool drop, bool @unsafe, bool create, bool migrate, bool applyMissing, bool pre, bool post)
        {
            _builder = builder;
            _connectionString = connectionString;
            _version = version;
            _drop = drop;
            _unsafe = @unsafe;
            _create = create;
            _migrate = migrate;
            _applyMissing = applyMissing;
            _pre = pre;
            _post = post;
        }

        async Task ICliCommand.RunAsync(CancellationToken cancel)
        {
            int exit = 0;

            if (!string.IsNullOrWhiteSpace(_connectionString))
                _builder = _builder.WithConnectionString(_connectionString);

            if (string.IsNullOrWhiteSpace(_builder.ConnectionString))
            {
                ConsoleMessages.WriteError("Connection string is required. Configure the connection string in the app or use --connectionstring <connectionstring>.");
                return;
            }

            MigrationPhase? phase;
            if (_pre && _post)
            {
                ConsoleMessages.WriteError("The --pre and --post options cannot both be used.");
                return;
            }
            else
            {
                phase = _pre ? MigrationPhase.Pre :
                    _post ? MigrationPhase.Post :
                    null;
            }

            var db = _builder.Build();

            ConsoleMessages.WriteLogo();
            await ConsoleMessages.WriteDatabaseInfo(db);

            try
            {

                if (_drop)
                {
                    if (_unsafe)
                        ConsoleMessages.WriteWarning("UNSAFE IS ON: Production keyword protections are disabled.");

                    Console.Write(" Dropping database...");
    
                    try
                    {
                        await db.Drop(_unsafe);
                    }
                    catch
                    {
                        ConsoleMessages.WriteColorLine(" FAILED ", ConsoleColor.White, ConsoleColor.Red);
                        throw;
                    }

                    ConsoleMessages.WriteColorLine("Success", ConsoleColor.Green);
                    Console.WriteLine();
                }

                if (_create)
                {
                    Console.Write(" Creating database...");

                    try
                    { 
                        await db.Create();
                    }
                    catch
                    {
                        ConsoleMessages.WriteColorLine(" FAILED ", ConsoleColor.White, ConsoleColor.Red);
                        throw;
                    }

                    ConsoleMessages.WriteColorLine("Success", ConsoleColor.Green);
                    Console.WriteLine();
                }

                if (_migrate)
                {
                    await db.MigrateTo(_version, phase, _applyMissing);
                }
            }
            catch (SqlExceptionWithSource ex)
            {
                Console.WriteLine();
                ConsoleMessages.WriteColorLine($" {ex.Message}", ConsoleColor.Red);
                Console.WriteLine();

                exit = -1;
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine();
                ConsoleMessages.WriteColorLine($" {ex.Message}", ConsoleColor.Red);
                Console.WriteLine();

                exit = -1;
            }
            catch (MissingMigrationsException ex)
            {
                Console.WriteLine();
                ConsoleMessages.WriteColorLine(" Cannot apply migrations to database. Migrations have been applied out of order and the following migrations are missing from the database:", ConsoleColor.Red);
                Console.WriteLine();

                foreach (var v in ex.MissingScripts.GroupBy(m => m.Version))
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

                exit = -1;
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                ConsoleMessages.WriteColorLine("   **   UNEXPECTED ERROR   **   ", ConsoleColor.White, ConsoleColor.Red);
                ConsoleMessages.WriteColorLine(ex.ToString(), ConsoleColor.Red);
                Console.WriteLine();

                exit = -1;
            }

            await ConsoleMessages.WriteDatabaseInfo(db);
            Console.WriteLine();

            Environment.Exit(exit);
        }
    }
}
