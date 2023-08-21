using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace WillSoss.Data
{
    public partial class DatabaseBuilder
    {
        private static readonly Regex NamedScriptPattern = GetNamedScriptPattern();

        private readonly Func<DatabaseBuilder, Database> _build;
        private readonly Dictionary<Version, Script> _migrations = new();
        private readonly List<string> _productionKeywords = new() { "prod", "live" };
        private readonly Dictionary<string, (Script? Script, Func<Database, Task>? Action)> _actions = new();

        public string? ConnectionString { get; private set; }
        public Script CreateScript { get; private set; }
        public Script DropScript { get; private set; }
        public Script? ResetScript { get; private set; }
        public IReadOnlyDictionary<string, Script> NamedScripts => GetNamedScripts();
        public IReadOnlyDictionary<string, Func<Database, Task>> Actions => _actions.Where(kv => kv.Value.Action is not null).ToDictionary(kv => kv.Key, kv => kv.Value.Action);
        public int CommandTimeout { get; private set; } = 90;
        public int PostCreateDelay { get; private set; } = 0;
        public int PostDropDelay { get; private set; } = 0;
        public IEnumerable<Script> MigrationScripts => _migrations.Values;
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
            CreateScript = create;
            DropScript = drop;
        }

        public DatabaseBuilder WithConnectionString(string connectionString)
        {
            ConnectionString = connectionString;
            return this;
        }

        public DatabaseBuilder AddMigrations(string directory)
        {
            foreach (var script in new ScriptDirectory(directory).Scripts)
            {
                AddMigration(script);
            }

            return this;
        }

        public DatabaseBuilder AddMigration(string path) => AddMigration(new Script(path));

        public DatabaseBuilder AddMigration(Script script)
        {
            if (_migrations.ContainsKey(script.Version))
                throw new InvalidScriptNameException(Path.Combine(script.Location, script.FileName), $"Version {script.Version} cannot be used more than once.");

            _migrations.Add(script.Version, script);

            return this;
        }

        public DatabaseBuilder WithCreateScript(string path) => WithCreateScript(new Script(path));

        public DatabaseBuilder WithCreateScript(Script script)
        {
            CreateScript = script;
            return this;
        }

        public DatabaseBuilder WithDropScript(string path) => WithDropScript(new Script(path));

        public DatabaseBuilder WithDropScript(Script script)
        {
            DropScript = script;
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
