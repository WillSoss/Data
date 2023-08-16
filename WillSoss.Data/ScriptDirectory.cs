namespace WillSoss.Data
{
    public class ScriptDirectory
    {
        readonly List<Script> scripts = new List<Script>();

        public string Path { get; }
        public IEnumerable<Script> Scripts => scripts.OrderBy(s => s.Version);

        public ScriptDirectory(string path)
        { 
            if (string.IsNullOrWhiteSpace(path)) 
                throw new ArgumentNullException(nameof(path));

            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException($"Directory not found: {path}");

            Path = path;

            foreach (var file in Directory.EnumerateFiles(Path, "*.sql"))
            {
                var script = new Script(file);

                if (!script.IsVersioned)
                    throw new InvalidScriptNameException(file, "Scripts must be named in the format '#[.#[.#[.#]]]-name.sql'");

                scripts.Add(script);
            }
        }
    }
}
