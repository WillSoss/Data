using WillSoss.DbDeploy.Cli;
using WillSoss.DbDeploy;

namespace WillSoss.DbDeploy.Cli
{
    internal class ResetCommand : ICliCommand
    {
        private DatabaseBuilder _builder;
        private readonly string? _connectionString;
        private readonly bool _unsafe;

        public ResetCommand(DatabaseBuilder builder, string? connectionString, bool @unsafe)
        {
            _builder = builder;
            _connectionString = connectionString;
            _unsafe = @unsafe;
        }

        async Task ICliCommand.RunAsync(CancellationToken cancel)
        {
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

                Console.WriteLine();
                await ConsoleMessages.WriteDatabaseInfo(db);
                Console.WriteLine();

                if (_unsafe)
                {
                    ConsoleMessages.WriteWarning(" UNSAFE IS ON: Production keyword protections are disabled for destructive actions.");
                    Console.WriteLine();
                }

                try
                {
                    Console.Write($" Resetting database...");

                    await db.Reset(_unsafe);

                    ConsoleMessages.WriteColorLine("Success", ConsoleColor.Green);
                    Console.WriteLine();
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
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine();
                ConsoleMessages.WriteColorLine($" {ex.Message}", ConsoleColor.Red);
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                ConsoleMessages.WriteColorLine("   **   UNEXPECTED ERROR   **   ", ConsoleColor.White, ConsoleColor.Red);
                ConsoleMessages.WriteColorLine(ex.ToString(), ConsoleColor.Red);
                Console.WriteLine();
            }
        }
    }
}
