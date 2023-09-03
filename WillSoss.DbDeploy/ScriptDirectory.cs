namespace WillSoss.DbDeploy
{
    public class ScriptDirectory
    {
        private readonly List<MigrationScript> _scripts = new();

        public string Path { get; }
        public IEnumerable<MigrationScript> Scripts => _scripts.OrderBy(s => s.Number);

        public ScriptDirectory(Version version, MigrationPhase phase, string path)
        { 
            if (string.IsNullOrWhiteSpace(path)) 
                throw new ArgumentNullException(nameof(path));

            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException($"Directory not found: {path}");

            Path = path;

            foreach (var file in Directory.EnumerateFiles(Path, "*.sql"))
            {
                var script = new MigrationScript(version, phase, file);

                _scripts.Add(script);
            }
        }
    }
}
