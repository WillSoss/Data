namespace WillSoss.Data
{
    public class ScriptDirectory
    {
        readonly List<(Version Version, Script Script)> scripts = new List<(Version, Script)>();

        public string Path { get; }
        public IEnumerable<Script> Scripts => scripts.OrderBy(i => i.Version).Select(i => i.Script);

        public ScriptDirectory(string path)
        { 
            if (string.IsNullOrWhiteSpace(path)) 
                throw new ArgumentNullException(nameof(path));

            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException($"Directory not found: {path}");

            Path = path;

            foreach ((string ver, string file) in new VersionedScriptNameParser(Directory.EnumerateFiles(Path, "*.sql")))
            {
                // Version class requires at least major.minor
                var clean = ver.IndexOf('.') < 0 ? $"{ver}.0" : ver;

                var version = Version.Parse(clean);
                var script = new Script(file);

                scripts.Add((version, script));
            }
        }
    }
}
