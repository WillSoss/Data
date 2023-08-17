namespace WillSoss.Data
{
    public class DatabaseBuilder
    {
        readonly Func<DatabaseBuilder, Database> _build;
        readonly Dictionary<Version, Script> _migrations = new Dictionary<Version, Script>();
        readonly List<string> _productionKeywords = new List<string>() { "prod", "live" };

        public string ConnectionString { get; private set; }
        public Script CreateScript { get; private set; }
        public Script DropScript { get; private set; }
        public Script? ResetScript { get; private set; }
        public int CommandTimeout { get; private set; } = 90;
        public int PostCreateDelay { get; private set; } = 0;
        public int PostDropDelay { get; private set; } = 0;
        public IEnumerable<Script> MigrationScripts => _migrations.Values;
        public IEnumerable<string> ProductionKeywords => _productionKeywords;

        /// <summary>
        /// Creates a new DatabaseBuilder
        /// </summary>
        /// <param name="build">Build function that takes the <see cref="DatabaseBuilder"/> and initializes a <see cref="Database"/>.</param>
        /// <param name="create">The default create script.</param>
        /// <param name="drop">The default drop script.</param>
        public DatabaseBuilder(Func<DatabaseBuilder, Database> build, string connectionString, Script create, Script drop)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            _build = build;
            ConnectionString = connectionString;
            CreateScript = create;
            DropScript = drop;
        }

        public DatabaseBuilder AddMigrations(string directory)
        {
            foreach (var script in new ScriptDirectory(directory).Scripts)
            {
                AddMigration(script);
            }

            return this;
        }

        public DatabaseBuilder AddMigration(string path)
        {
            AddMigration(new Script(path));

            return this;
        }

        public DatabaseBuilder AddMigration(Script script)
        {
            if (_migrations.ContainsKey(script.Version))
                throw new InvalidScriptNameException(Path.Combine(script.Location, script.FileName), $"Version {script.Version} cannot be used more than once.");

            _migrations.Add(script.Version, script);

            return this;
        }

        public DatabaseBuilder WithCreateScript(Script script)
        {
            CreateScript = script;
            return this;
        }

        public DatabaseBuilder WithDropScript(Script script)
        {
            DropScript = script;
            return this;
        }

        public DatabaseBuilder WithResetScript(Script script)
        {
            ResetScript = script;
            return this;
        }

        public DatabaseBuilder ClearProductionKeywords()
        {
            _productionKeywords.Clear();
            return this;
        }

        public DatabaseBuilder AddProductionKeywords(params string[] keywords)
        {
            _productionKeywords.AddRange(keywords);
            return this;
        }

        public DatabaseBuilder WithCommandTimeout(int seconds)
        {
            CommandTimeout = seconds;
            return this;
        }

        public DatabaseBuilder WithPostCreateDelay(int seconds)
        {
            PostCreateDelay = seconds;
            return this;
        }

        public DatabaseBuilder WithPostDropDelay(int seconds)
        {
            PostDropDelay = seconds;
            return this;
        }

        public Database Build() => _build(this);
    }
}
