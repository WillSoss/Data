namespace WillSoss.DbDeploy
{
    public class VersionDirectory
    {
        public Version Version { get; }
        public string Name { get; }

        public ScriptDirectory? PreDeployment { get; private set; }
        public ScriptDirectory? PostDeployment { get; private set; }

        public IEnumerable<MigrationScript> Scripts =>
                (PreDeployment?.Scripts ?? Enumerable.Empty<MigrationScript>()).Union(
                    (PostDeployment?.Scripts ?? Enumerable.Empty<MigrationScript>()));

        public VersionDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(nameof(path));

            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException($"Directory not found: {path}");

            string? version;
            string? name;

            if (!Parser.TryParseFolderName(path, out version, out name))
                throw new InvalidFolderNameException(path, "Version folders must be named '[v]#.#[.#[.#]][ <name>]'.");

            Version = Version.Parse(version!);
            Name = name!;

            foreach (var dir in Directory.EnumerateDirectories(path))
            {
                string? dirName = Path.GetFileName(dir)?.TrimStart('_').ToLowerInvariant();

                if (dirName is null)
                    throw new ArgumentNullException(nameof(path), "Cannot be a root directory.");

                if (dirName.Equals("pre"))
                    PreDeployment = new ScriptDirectory(Version, MigrationPhase.Pre, dir);
                else if (dirName.Equals("post"))
                    PostDeployment = new ScriptDirectory(Version, MigrationPhase.Post, dir);
                else
                    throw new InvalidFolderNameException(dir, "Folder must be named '[_]pre' or 'post'.");
            }

            if (PreDeployment is null && PostDeployment is null)
                throw new InvalidFolderStructureException(path, "Must contain a folder named 'pre' and/or 'post'.");
        }
    }
}

