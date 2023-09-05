using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace WillSoss.DbDeploy
{
    public partial class DatabaseBuilder
    {
        private static readonly Regex NamedScriptPattern = GetNamedScriptPattern();

        private readonly Func<DatabaseBuilder, Database> _build;
        private readonly List<MigrationScript> _migrations = new();
        private readonly List<string> _productionKeywords = new() { "prod", "live" };
        private readonly Dictionary<string, (Script? Script, Func<Database, Task>? Action)> _actions = new();

        public Func<Database, Task<Script>> GetCreateScript;
        public Func<Database, Task<Script>> GetDropScript;
        public string? ConnectionString { get; private set; }
        public Script? ResetScript { get; private set; }
        public IReadOnlyDictionary<string, Script> NamedScripts => GetNamedScripts();
        public IReadOnlyDictionary<string, Func<Database, Task>> Actions => _actions.Where(kv => kv.Value.Action is not null).ToDictionary(kv => kv.Key, kv => kv.Value.Action);
        public int CommandTimeout { get; private set; } = 90;
        public int PostCreateDelay { get; private set; } = 0;
        public int PostDropDelay { get; private set; } = 0;
        public IEnumerable<MigrationScript> MigrationScripts => _migrations;
        public IEnumerable<string> ProductionKeywords => _productionKeywords;

        IReadOnlyDictionary<string, Script> GetNamedScripts()
        {
            Dictionary<string, Script> d = new();

            foreach (var kv in _actions.Where(kv => kv.Value.Script is not null))
                d.Add(kv.Key, kv.Value.Script!);

            return new ReadOnlyDictionary<string, Script>(d);
        }

        IReadOnlyDictionary<string, Func<Database, Task>> GetActions()
        {
            Dictionary<string, Func<Database, Task>> d = new();

            foreach (var kv in _actions.Where(kv => kv.Value.Action is not null))
                d.Add(kv.Key, kv.Value.Action!);

            return new ReadOnlyDictionary<string, Func<Database, Task>>(d);
        }

        /// <summary>
        /// Creates a new DatabaseBuilder
        /// </summary>
        /// <param name="build">Build function that takes the <see cref="DatabaseBuilder"/> and initializes a <see cref="Database"/>.</param>
        /// <param name="create">The default create script.</param>
        /// <param name="drop">The default drop script.</param>
        public DatabaseBuilder(Func<DatabaseBuilder, Database> build, Script create, Script drop)
        {
            _build = build;
            GetDropScript = new Func<Database, Task<Script>>(db => Task.FromResult(drop));
            GetCreateScript = new Func<Database, Task<Script>>(db => Task.FromResult(create));
        }

        public DatabaseBuilder(Func<DatabaseBuilder, Database> build, Func<Database, Task<Script>> create, Func<Database, Task<Script>> drop)
        {
            _build = build;
            GetDropScript = drop;
            GetCreateScript = create;
        }

        public DatabaseBuilder WithConnectionString(string connectionString)
        {
            ConnectionString = connectionString;
            return this;
        }

        public DatabaseBuilder AddMigrations(string directory)
        {
            foreach (var script in new MigrationsDirectory(directory).Scripts)
                AddMigration(script);

            return this;
        }

        public DatabaseBuilder AddMigration(MigrationScript script)
        {
            if (_migrations.Any(s => s.Version == script.Version && s.Phase == script.Phase && s.Number == script.Number))
                throw new InvalidScriptNameException(Path.Combine(script.Location, script.FileName), $"Migration scripts must have unique version, phase and numbers. {script.Version}/{script.Phase}/{script.Number} appears more than once.");

            _migrations.Add(script);

            return this;
        }
        public DatabaseBuilder AddMigrations(IEnumerable<MigrationScript> scripts)
        {
            foreach (var script in scripts)
                AddMigration(script);

            return this;
        }

        public DatabaseBuilder WithCreateScript(string path) => WithCreateScript(new Script(path));

        public DatabaseBuilder WithCreateScript(Script script)
        {
            GetCreateScript = new Func<Database, Task<Script>>(db => Task.FromResult(script));
            return this;
        }

        public DatabaseBuilder WithDropScript(string path) => WithDropScript(new Script(path));

        public DatabaseBuilder WithDropScript(Script script)
        {
            GetDropScript = new Func<Database, Task<Script>>(db => Task.FromResult(script));
            return this;
        }

        public DatabaseBuilder WithResetScript(string path) => WithResetScript(new Script(path));

        public DatabaseBuilder WithResetScript(Script script)
        {
            ResetScript = script;
            return this;
        }

        public DatabaseBuilder AddNamedScript(string name, string path) =>
            AddNamedScript(name, new Script(path));

        public DatabaseBuilder AddNamedScript(string name, Script script)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            if (!NamedScriptPattern.IsMatch(name))
                throw new ArgumentException("Name can only contain numbers, letters, dash (-), and underscore (_).");

            if (NamedScripts.Keys.Contains(name, StringComparer.InvariantCultureIgnoreCase))
                throw new ArgumentException("Named scripts and actions must have unique names.");

            _actions.Add(name, (script, null));

            return this;
        }

        public DatabaseBuilder AddAction(string name, Func<Database, Task> action)
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action));
            
            if (NamedScripts.Keys.Contains(name, StringComparer.InvariantCultureIgnoreCase))
                throw new ArgumentException("Named scripts and actions must have unique names.");

            _actions.Add(name, (null, action));

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

        public Database Build()
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
                throw new ArgumentNullException(nameof(ConnectionString));
            
            return _build(this);
        }

        [GeneratedRegex("^[-\\w]+$", RegexOptions.Compiled | RegexOptions.Singleline)]
        private static partial Regex GetNamedScriptPattern();
    }
}
