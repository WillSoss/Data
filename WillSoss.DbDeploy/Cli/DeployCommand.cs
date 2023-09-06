using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;

namespace WillSoss.DbDeploy.Cli
{
    internal class DeployCommand : RootCommand, ICliCommand
    {
        private DatabaseBuilder _builder;
        private readonly string? _connectionString;
        private readonly Version? _version;
        private readonly bool _drop;
        private readonly bool _unsafe;
        private readonly bool _create;
        private readonly bool _migrate;
        private readonly bool _pre;
        private readonly bool _post;
        private readonly ILogger _logger;

        public DeployCommand(DatabaseBuilder builder, string? connectionString, Version? version, bool drop, bool @unsafe, bool create, bool migrate, bool pre, bool post, ILogger<DeployCommand> logger)
        {
            _builder = builder;
            _connectionString = connectionString;
            _version = version;
            _drop = drop;
            _unsafe = @unsafe;
            _create = create;
            _migrate = migrate;
            _pre = pre;
            _post = post;
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

            MigrationPhase? phase;
            if (_pre && _post)
            {
                _logger.LogError("The --pre and --post options cannot both be used.");
                return;
            }
            else
            {
                phase = _pre ? MigrationPhase.Pre :
                    _post ? MigrationPhase.Post :
                    null;
            }

            var db = _builder.Build();

            Console.WriteLine();
            await ConsoleMessages.WriteDatabaseInfo(db);
            Console.WriteLine();

            try
            {

                if (_drop)
                {
                    if (_unsafe)
                        _logger.LogWarning("UNSAFE IS ON: Production keyword protections are disabled.");

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
                    await db.MigrateTo(_version, phase);
                }
            }
            catch (SqlExceptionWithSource ex)
            {
                Console.WriteLine();
                ConsoleMessages.WriteColorLine(ex.Message, ConsoleColor.Red);
                Console.WriteLine();
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine();
                ConsoleMessages.WriteColorLine(ex.Message, ConsoleColor.Red);
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                ConsoleMessages.WriteColorLine("   **   UNEXPECTED ERROR   **   ", ConsoleColor.White, ConsoleColor.Red);
                ConsoleMessages.WriteColorLine(ex.ToString(), ConsoleColor.Red);
                Console.WriteLine();
            }

            await ConsoleMessages.WriteDatabaseInfo(db);
        }
    }
}
