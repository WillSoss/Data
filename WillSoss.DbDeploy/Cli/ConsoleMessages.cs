namespace WillSoss.DbDeploy.Cli
{
    internal static class ConsoleMessages
    {
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

        internal static void WriteError(string text) => WriteColorLine(text, ConsoleColor.Red);

        internal static void WriteWarning(string text) => WriteColorLine(text, ConsoleColor.DarkYellow);

        internal static void WriteColorLine(string text, ConsoleColor foreground, ConsoleColor? background = null)
        {
            WriteColor(text, foreground, background);
            Console.WriteLine();
        }

        private const int InnerWidth = 40;
        
        internal static void WriteLogo()
        {
            Console.WriteLine();
            WriteColorLine("""
 ____    __       ____                    ___                            
/\  _`\ /\ \     /\  _`\                 /\_ \                           
\ \ \/\ \ \ \____\ \ \/\ \     __   _____\//\ \     ___   __  __         
 \ \ \ \ \ \ '__`\\ \ \ \ \  /'__`\/\ '__`\\ \ \   / __`\/\ \/\ \        
  \ \ \_\ \ \ \L\ \\ \ \_\ \/\  __/\ \ \L\ \\_\ \_/\ \L\ \ \ \_\ \       
   \ \____/\ \_,__/ \ \____/\ \____\\ \ ,__//\____\ \____/\/`____ \      
    \/___/  \/___/   \/___/  \/____/ \ \ \/ \/____/\/___/  `/___/> \     
                                      \ \_\                   /\___/     
                                       \/_/                   \/__/ 
""", ConsoleColor.Magenta);
            Console.WriteLine();
        }
        internal static async Task WriteDatabaseInfo(Database db)
        {

            Console.WriteLine(" Database Status");
            Console.WriteLine();

            Console.WriteLine($"   Host:             {db.GetServerName()}");
            Console.WriteLine($"   Database:         {db.GetDatabaseName()}");
            Console.WriteLine($"   Is Production:    {(db.IsProduction() ? "Yes" : "No")}");
            Console.Write($"   Database Exists:  ");

            bool exists = false;
            try
            {
                exists = await db.Exists();
                Console.WriteLine(exists ? "Yes" : "No");
            }
            catch
            {
                WriteColorLine("Could not determine (check host/credentials)", ConsoleColor.Red);
            }

            var v = exists ? await db.GetVersion() : null;
            var partial = exists ? (await db.GetUnappliedMigrations()).Any(m => m.Version == v) : false;
            Console.WriteLine($"   Current Version:  {(v is null ? "---" : $"v{v}")}{(partial ? "-partial" : string.Empty)}");
            
            Console.WriteLine();

        }

        private static void StartBox(string? title)
        {
            Console.Write("╔");
            Console.Write(GetPadded($" {title} ", '═'));
            Console.WriteLine("╗");
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
