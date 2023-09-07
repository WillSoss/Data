namespace WillSoss.DbDeploy.Cli
{
    internal class RunCommand : ICliCommand
    {
        private DatabaseBuilder _builder;
        private readonly string? _connectionString;
        private readonly string? _action;

        public RunCommand(DatabaseBuilder builder, string? connectionString, string? action)
        {
            _builder = builder;
            _connectionString = connectionString;
            _action = action;
        }

        async Task ICliCommand.RunAsync(CancellationToken cancel)
        {
            int exit = 0;

            try
            {
                if (!string.IsNullOrWhiteSpace(_connectionString))
                    _builder = _builder.WithConnectionString(_connectionString);

                if (string.IsNullOrWhiteSpace(_builder.ConnectionString))
                {
                    ConsoleMessages.WriteError(" Connection string is required. Configure the connection string in the app or use --connectionstring <connectionstring>.");
                    return;
                }

                var db = _builder.Build();

                ConsoleMessages.WriteLogo();
                await ConsoleMessages.WriteDatabaseInfo(db);

                var scriptKey = db.NamedScripts.Keys.FirstOrDefault(k => k.Equals(_action, StringComparison.InvariantCultureIgnoreCase));
                var actionKey = db.Actions.Keys.FirstOrDefault(k => k.Equals(_action, StringComparison.InvariantCultureIgnoreCase));

                try
                {
                    if (scriptKey is not null)
                    {
                        var script = db.NamedScripts[scriptKey!];

                        Console.Write($" Running script ");
                        ConsoleMessages.WriteColor(script.FileName, ConsoleColor.Blue);
                        Console.Write("...");

                        await db.ExecuteScriptAsync(script, db.GetConnection());

                        ConsoleMessages.WriteColorLine("Success", ConsoleColor.Green);
                        Console.WriteLine();
                    }
                    else if (actionKey is not null)
                    {
                        var action = db.Actions[actionKey!];

                        Console.Write($" Running action ");
                        ConsoleMessages.WriteColor(actionKey, ConsoleColor.Blue);
                        Console.Write("...");

                        await action(db);

                        ConsoleMessages.WriteColorLine("Success", ConsoleColor.Green);
                        Console.WriteLine();
                    }
                    else
                    {
                        ConsoleMessages.WriteError($" A script or action named {_action} could not be found.");
                        Console.WriteLine();
                    }
                }
                catch
                {
                    ConsoleMessages.WriteColorLine(" FAILED ", ConsoleColor.White, ConsoleColor.Red);
                    throw;
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
            catch (Exception ex)
            {
                Console.WriteLine();
                ConsoleMessages.WriteColorLine("   **   UNEXPECTED ERROR   **   ", ConsoleColor.White, ConsoleColor.Red);
                ConsoleMessages.WriteColorLine(ex.ToString(), ConsoleColor.Red);
                Console.WriteLine();

                exit = -1;
            }

            Environment.Exit(exit);
        }
    }
}
