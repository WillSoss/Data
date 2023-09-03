namespace WillSoss.DbDeploy
{
    public class MigrationScript : Script
    {
        public Version Version { get; }
        public MigrationPhase Phase { get; }
        public int Number { get; }
        public string Description { get; }

        public MigrationScript(Version version, MigrationPhase phase, string path)
            : base(path)
        {
            Version = version;
            Phase = phase;

            if (Parser.TryParseFileName(path, out string? number, out string? name))
            {
                Number = int.Parse(number!);
                Description = name!;
            }
            else
            {
                throw new InvalidScriptNameException(path, "Migration scripts must be named in the format '#[ <name>].sql'");
            }
        }

        public override string ToString() => $"{Version}/{Phase}/{Number}";

        public static bool operator <(MigrationScript left, Migration right)
        {
            if (left.Version < right.Version)
                return true;

            if (left.Version == right.Version && left.Phase < right.Phase)
                return true;

            if (left.Version == right.Version && left.Phase == right.Phase && left.Number < right.Number)
                return true;

            return false;
        }

        public static bool operator >(MigrationScript left, Migration right)
        {
            if (left.Version > right.Version)
                return true;

            if (left.Version == right.Version && left.Phase > right.Phase)
                return true;

            if (left.Version == right.Version && left.Phase == right.Phase && left.Number > right.Number)
                return true;

            return false;
        }
    }
}
