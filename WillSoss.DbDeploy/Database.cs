using System.Data;
using System.Data.Common;

namespace WillSoss.DbDeploy
{
    public abstract class Database
    {
        private readonly IEnumerable<Script> _migrations;
        private readonly string[] _productionKeywords;
        private readonly int _commandTimeout;
        private readonly int _postCreateDelay;
        private readonly int _postDropDelay;

        public string? ConnectionString { get; }
        public int CommandTimeout => _commandTimeout;
        public Script CreateScript { get; }
        public IEnumerable<Script> Migrations => _migrations.OrderBy(s => s.Version);
        public Script? ResetScript { get; }
        public Script DropScript { get; }
        public IReadOnlyDictionary<string, Script> NamedScripts { get; }
        public IReadOnlyDictionary<string, Func<Database, Task>> Actions { get; }

        internal Database(DatabaseBuilder builder)
        {
            ConnectionString = builder.ConnectionString;
            CreateScript = builder.CreateScript;
            DropScript = builder.DropScript;
            ResetScript = builder.ResetScript;
            _migrations = builder.MigrationScripts;
            _commandTimeout = builder.CommandTimeout;
            _postCreateDelay = builder.PostCreateDelay ;
            _postDropDelay = builder.PostDropDelay;
            _productionKeywords = builder.ProductionKeywords.Distinct().ToArray();
            NamedScripts = builder.NamedScripts;
            Actions = builder.Actions;
        }

        public virtual async Task Create()
        {
            using (var db = GetConnectionWithoutDatabase())
            {
                await ExecuteScriptAsync(CreateScript, db, replacementTokens: GetTokens());
            }

            if (_postCreateDelay > 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(_postCreateDelay));
            }

            using (var db = GetConnection())
            {
                await ExecuteScriptAsync(GetMigrationsTableScript(), db, replacementTokens: GetTokens());
            }
        }

        /// <summary>
        /// Builds the database using the <see cref="Migrations"/>.
        /// </summary>
        /// <returns>The number of scripts applied to the database.</returns>
        public virtual async Task<int> MigrateToLatest() => await MigrateTo(null);

        /// <summary>
        /// Builds the database using the <see cref="Migrations"/>.
        /// </summary>
        /// <param name="version">Applies builds scripts up to the specified version.</param>
        /// <returns>The number of scripts applied to the database.</returns>
        public virtual async Task<int> MigrateTo(Version? version)
        {
            version = version?.FillZeros();

            using var db = GetConnection();

            await db.EnsureOpenAsync();

            using var tx = db.BeginTransaction();

            var applied = (await GetAppliedMigrations(db, tx)).Select(m => m.Version);

            var latestApplied = applied.Max();

            var scriptsToApply = Migrations.Where(s => !applied.Contains(s.Version));

            if (version is not null)
                scriptsToApply = scriptsToApply.Where(s => s.Version <= version);

            if (scriptsToApply.Any())
            {
                var scriptVersion = scriptsToApply.Select(m => m.Version).Min();

                if (latestApplied is not null && scriptVersion is not null && scriptVersion < latestApplied)
                    throw new MigrationsNotAppliedInOrderException(latestApplied, scriptVersion);

                await ExecuteScriptsAsync(scriptsToApply, db, tx, GetTokens(), true);

                await tx.CommitAsync();
            }

            return scriptsToApply.Count();
        }

        public virtual async Task Reset(bool @unsafe = false)
        {
            using var db = GetConnection();

            if (!@unsafe && IsProd(db))
                throw new InvalidOperationException("Cannot reset a production database. The connection string contains a production keyword.");

            if (ResetScript is null)
                return;

            await db.EnsureOpenAsync();

            using var tx = db.BeginTransaction();

            await ExecuteScriptAsync(ResetScript, db, tx, GetTokens());

            await tx.CommitAsync();
        }

        public virtual async Task Drop(bool @unsafe = false)
        {
            using var db = GetConnectionWithoutDatabase();

            if (!@unsafe && IsProd(db))
                throw new InvalidOperationException("Cannot drop a production database. The connection string contains a production keyword.");

            await ExecuteScriptAsync(DropScript, db, replacementTokens: GetTokens());

            if (_postDropDelay > 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(_postDropDelay));
            }
        }

        private bool IsProd(DbConnection db)
        {
            var cs = db.ConnectionString.ToLower();

            return _productionKeywords.Any(k => cs.Contains(k, StringComparison.InvariantCultureIgnoreCase));
        }

        public virtual async Task ExecuteNamedScript(string name)
        {
            if (!NamedScripts.ContainsKey(name))
                throw new ArgumentException($"Script {name} not found.");

            using var db = GetConnection();

            await db.EnsureOpenAsync();

            using var tx = db.BeginTransaction();

            await ExecuteScriptAsync(NamedScripts[name], db, tx, GetTokens());

            await tx.CommitAsync();
        }

        public async Task ExecuteScriptsAsync(IEnumerable<Script> scripts, DbConnection db, DbTransaction? tx = null, Dictionary<string, string>? replacementTokens = null, bool recordMigration = false)
        {
            foreach (var script in scripts)
            {
                await ExecuteScriptAsync(script, db, tx, replacementTokens);

                if (recordMigration)
                    await RecordMigration(script, db, tx);
            }
        }

        public async Task ExecuteScriptAsync(Script script, DbConnection db, DbTransaction? tx = null, Dictionary<string, string>? replacementTokens = null)
        {
            foreach (string batch in script.Batches)
                await ExecuteScriptAsync(batch, db, tx, replacementTokens);
        }

        public async Task ExecuteScriptAsync(string sql, DbConnection db, DbTransaction? tx = null, Dictionary<string, string>? replacementTokens = null)
        {
            if (replacementTokens != null)
            {
                foreach (var token in replacementTokens)
                    sql = sql.Replace($"{{{{{token.Key}}}}}", token.Value);
            }

            await ExecuteScriptAsync(sql, db, tx);
        }

        protected abstract Task ExecuteScriptAsync(string sql, DbConnection db, DbTransaction? tx = null);

        protected Dictionary<string, string> GetTokens() => new()
        {
            { "database", GetDatabaseName() }
        };

        protected internal abstract DbConnection GetConnection();
        protected internal abstract DbConnection GetConnectionWithoutDatabase();
        protected internal abstract string GetDatabaseName();
        protected internal abstract string GetServerName();
        protected internal abstract Script GetMigrationsTableScript();
        protected internal abstract Task<IEnumerable<Migration>> GetAppliedMigrations(DbConnection db, DbTransaction? tx = null);
        protected internal abstract Task RecordMigration(Script script, DbConnection db, DbTransaction? tx = null);
    }
}
