namespace WillSoss.DbDeploy
{
    public class MigrationsDirectory
    {
        private readonly List<VersionDirectory> _versions = new();

        public string Path { get; }
        public IEnumerable<MigrationScript> Scripts => _versions.OrderBy(d => d.Version).SelectMany(d =>  d.Scripts);

        public MigrationsDirectory(string path)
        { 
            if (string.IsNullOrWhiteSpace(path)) 
                throw new ArgumentNullException(nameof(path));

            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException($"Directory not found: {path}");

            Path = path;

            foreach (string dir in Directory.EnumerateDirectories(path))
                _versions.Add(new VersionDirectory(dir));
        }
    }
}
