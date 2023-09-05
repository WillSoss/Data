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

            Console.WriteLine();

            await WriteDatabaseInfo(db);

            Console.WriteLine();

            if (db.IsProduction())
                WriteCaution("You are targetting a production database!");

            if (unapplied?.Count() == 0)
            {
                Console.WriteLine("There are no unapplied migrations. The database is up to date.");
            }
            else
            {
                Console.WriteLine("Unapplied migrations:");
                Console.WriteLine();

                foreach (var v in unapplied!.GroupBy(m => m.Version))
                {
                    Console.WriteLine($"v{v.Key}");

                    foreach (var p in v.GroupBy(m => m.Phase))
                    {
                        Console.WriteLine($"  {p.Key}");
                        foreach (var s in v)
                        {
                            Console.WriteLine($"    {s.FileName}");
                        }

                        Console.WriteLine();
                    }
                }
            }
        }

        private const int InnerWidth = 40;
        private async Task WriteDatabaseInfo(Database db)
        {
            StartBox("Database Status");

            WriteBoxedText($" Host:             {db.GetServerName()}");
            WriteBoxedText($" Database:         {db.GetDatabaseName()}");
            WriteBoxedText($" Is Production:    {db.IsProduction()}");
            WriteBoxedText($" Database Exists:  {await db.Exists()}");
            var v = await db.GetVersion();
            WriteBoxedText($" Current Version:  {(v is null ? "---" : $"v{v}")}");

            EndBox();
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

        private void StartBox(string? title)
        {
            Console.Write("╔");
            Console.Write(GetPadded($" {title} ", '═'));
            Console.WriteLine("╗");
        }

        private void EndBox()
        {
            Console.Write("╚");
            Console.Write(GetPadded(null, '═'));
            Console.WriteLine("╝");
        }

        private void WriteBoxedText(string text, int length = InnerWidth, bool truncate = true)
        {
            Console.Write('║');
            Console.Write(GetPadded(text));
            Console.WriteLine('║');
        }

        private string GetPadded(string? text, char padding = ' ', int length = InnerWidth, bool truncate = true)
        {
            text = text ?? string.Empty;

            if (text.Length > length)
                text = text.Substring(0, length);
            else if (text.Length < length)
                text = text + new string(padding, length - text.Length);

            return text;
        }
    }
}
