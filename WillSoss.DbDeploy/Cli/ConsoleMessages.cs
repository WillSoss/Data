using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace WillSoss.DbDeploy.Cli
{
    internal static class ConsoleMessages
    {
        internal static void WriteMigrationAppliedSuccessfully(MigrationScript script)
        {
            var foreground = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Green;

            Console.Write($"✓ {script.FileName}");

            Console.ForegroundColor = foreground;

            Console.WriteLine();
            Console.WriteLine();
        }

        internal static void WriteMigrationFailed(MigrationScript script, SqlExceptionWithSource ex)
        {
            var foreground = Console.ForegroundColor;
            var background = Console.BackgroundColor;

            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Red;

            Console.Write($"× {script.FileName}");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = background;

            Console.WriteLine();

            Console.Write(ex.Message);

            Console.ForegroundColor = foreground;

            Console.WriteLine();
        }

        internal static void WriteColor(string text, ConsoleColor foreground, ConsoleColor? background = null)
        {
            var originalForeground = Console.ForegroundColor;
            var originalBackground = Console.BackgroundColor;

            Console.ForegroundColor = foreground;
            Console.BackgroundColor = background ?? originalBackground;

            Console.Write(text);

            Console.ForegroundColor = originalForeground;
            Console.BackgroundColor = originalBackground;
        }

        internal static void WriteColorLine(string text, ConsoleColor foreground, ConsoleColor? background = null)
        {
            WriteColor(text, foreground, background);
            Console.WriteLine();
        }

        private const int InnerWidth = 40;
        internal static async Task WriteDatabaseInfo(Database db)
        {
            StartBox("Database Status");

            WriteBoxedText($" Host:             {db.GetServerName()}");
            WriteBoxedText($" Database:         {db.GetDatabaseName()}");
            WriteBoxedText($" Is Production:    {(db.IsProduction() ? "Yes" : "No")}");
            WriteBoxedText($" Database Exists:  {((await db.Exists()) ? "Yes" : "No")}");
            var v = await db.GetVersion();
            var partial = (await db.GetUnappliedMigrations()).Any(m => m.Version == v);
            WriteBoxedText($" Current Version:  {(v is null ? "---" : $"v{v}")}{(partial ? "-partial" : string.Empty)}");

            EndBox();
        }

        private static void StartBox(string? title)
        {
            Console.Write("╔");
            Console.Write(GetPadded($" {title} ", '═'));
            Console.WriteLine("╗");
        }

        private static void EndBox()
        {
            Console.Write("╚");
            Console.Write(GetPadded(null, '═'));
            Console.WriteLine("╝");
        }

        private static void WriteBoxedText(string text, int length = InnerWidth, bool truncate = true)
        {
            Console.Write('║');
            Console.Write(GetPadded(text));
            Console.WriteLine('║');
        }

        private static string GetPadded(string? text, char padding = ' ', int length = InnerWidth, bool truncate = true)
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
