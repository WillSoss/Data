using System;
using System.Data;
using System.Data.Common;
using System.Net.Security;
using WillSoss.DbDeploy.Cli;

namespace WillSoss.DbDeploy
{
    public abstract class Database
    {
        private readonly IEnumerable<MigrationScript> _migrations;
        private readonly string[] _productionKeywords;
        private readonly int _commandTimeout;
        private readonly int _postCreateDelay;
        private readonly int _postDropDelay;

        private Func<Database, Task<Script>> _create;
        private Func<Database, Task<Script>> _drop;

        public string? ConnectionString { get; }
        public int CommandTimeout => _commandTimeout;
        public IEnumerable<MigrationScript> Migrations => 
            _migrations
            .OrderBy(s => s.Version)
            .ThenBy(s => s.Phase)
            .ThenBy(s => s.Number);

        public Script? ResetScript { get; }
        public IReadOnlyDictionary<string, Script> NamedScripts { get; }
        public IReadOnlyDictionary<string, Func<Database, Task>> Actions { get; }

        internal Database(DatabaseBuilder builder)
        {
            ConnectionString = builder.ConnectionString;
            ResetScript = builder.ResetScript;
            _migrations = builder.MigrationScripts;
            _commandTimeout = builder.CommandTimeout;
            _postCreateDelay = builder.PostCreateDelay ;
            _postDropDelay = builder.PostDropDelay;
            _productionKeywords = builder.ProductionKeywords.Distinct().ToArray();
            NamedScripts = builder.NamedScripts;
            Actions = builder.Actions;

            _create = builder.GetCreateScript;
            _drop = builder.GetDropScript;
        }

        public Task<Script> GetCreateScript() => _create(this);
        public Task<Script> GetDropScript() => _drop(this);

        public virtual async Task Create()
        {
            using (var db = GetConnectionWithoutDatabase())
            {
                await ExecuteScriptAsync(await GetCreateScript(), db, replacementTokens: GetTokens());
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
        public virtual async Task<int> MigrateToLatest() => await MigrateTo(null, null);

        /// <summary>
        /// Builds the database using the <see cref="Migrations"/>.
        /// </summary>
        /// <param name="version">Applies builds scripts up to the specified version.</param>
        /// <returns>The number of scripts applied to the database.</returns>
        public virtual async Task<int> MigrateTo(Version? version, MigrationPhase? phase = null)
        {
            using var db = GetConnection();

            await db.EnsureOpenAsync();

            using var tx = db.BeginTransaction();

            var applied = await GetAppliedMigrations(db, tx);

            var latestApplied = applied.OrderBy(a => a.Version).ThenBy(a => a.Phase).ThenBy(a => a.Number).LastOrDefault();

            var scriptsToApply = Migrations.Where(s => !applied.Any(a => a.Version == s.Version && a.Phase == s.Phase && a.Number == s.Number));

            if (version is not null)
                scriptsToApply = scriptsToApply.Where(s => s.Version <= version);

            if (scriptsToApply.Any())
            {
                var scriptVersion = scriptsToApply.OrderBy(a => a.Version).ThenBy(a => a.Phase).ThenBy(a => a.Number).FirstOrDefault();

                if (latestApplied is not null && scriptVersion is not null && scriptVersion < latestApplied)
                    throw new MigrationsNotAppliedInOrderException(latestApplied, scriptVersion);

                scriptsToApply = FilterScripts(scriptsToApply, phase);

                await MigrateAsync(scriptsToApply, db, tx, GetTokens());

                await tx.CommitAsync();
            }

            return scriptsToApply.Count();
        }

        private IEnumerable<MigrationScript> FilterScripts(IEnumerable<MigrationScript> scripts, MigrationPhase? phase)
        {
            List<MigrationScript> filteredScripts = new();

            if (phase == MigrationPhase.Pre)
            {
                bool foundPost = false;
                foreach (var script in scripts)
                {
                    if (script.Phase == MigrationPhase.Post)
                        foundPost = true;
                    else if (foundPost)
                        throw new UnableToMigrateException("Unable to apply migration scripts. When applying only pre-migration scripts, a pre-deployment script cannot come after a post-migration script. Try specifying the version to migrate to if migrating to latest is not desired.");
                    else
                        filteredScripts.Add(script);
                }

                return filteredScripts;
            }
            else if (phase == MigrationPhase.Post)
            {
                foreach (var script in scripts)
                {
                    if (script.Phase == MigrationPhase.Pre)
                        throw new UnableToMigrateException("Unable to apply migration scripts. When applying only post-migration scripts, all pre-deployment scripts must already be applied. Try specifying the version to migrate to if migrating to latest is not desired.");

                    filteredScripts.Add(script);
                }

                return filteredScripts;
            }
            else
            {
                return scripts;
            }
        }

        public virtual async Task Reset(bool @unsafe = false)
        {
            using var db = GetConnection();

            if (!@unsafe && IsProduction())
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

            if (!@unsafe && IsProduction())
                throw new InvalidOperationException("Cannot drop a production database. The connection string contains a production keyword.");

            await ExecuteScriptAsync(await GetDropScript(), db, replacementTokens: GetTokens());

            if (_postDropDelay > 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(_postDropDelay));
            }
        }

        public bool IsProduction(DbConnection? db = null)
        {
            var cs = (db ?? GetConnection()).ConnectionString.ToLower();

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

        public async Task ExecuteScriptsAsync(IEnumerable<Script> scripts, DbConnection db, DbTransaction? tx = null, Dictionary<string, string>? replacementTokens = null)
        {
            foreach (var script in scripts)
            {
                await ExecuteScriptAsync(script, db, tx, replacementTokens);
            }
        }

        private async Task MigrateAsync(IEnumerable<MigrationScript> scripts, DbConnection db, DbTransaction? tx = null, Dictionary<string, string>? replacementTokens = null)
        {
            foreach (var v in scripts.GroupBy(m => m.Version))
            {
                Console.WriteLine($" Migrating to v{v.Key}");
                Console.WriteLine();

                foreach (var p in v.GroupBy(v => v.Phase))
                {
                    Console.WriteLine($"   {p.Key}-deployment scripts");

                    foreach (var script in p)
                    {
                        Console.Write($"     Applying ");
                        ConsoleMessages.WriteColor(script.FileName, ConsoleColor.Blue);
                        Console.Write("...");

                        try
                        {
                            await ExecuteScriptAsync(script, db, tx, replacementTokens);
                        }
                        catch (SqlExceptionWithSource)
                        {
                            ConsoleMessages.WriteColorLine(" FAILED ", ConsoleColor.White, ConsoleColor.Red);
                            throw;
                        }

                        await RecordMigration(script, db, tx);

                        ConsoleMessages.WriteColorLine("Success", ConsoleColor.Green);
                    }

                    Console.WriteLine();
                }
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

        public abstract Task<bool> Exists();
        protected internal abstract DbConnection GetConnection();
        protected internal abstract DbConnection GetConnectionWithoutDatabase();
        protected internal abstract string GetDatabaseName();
        protected internal abstract string GetServerName();
        protected internal abstract Script GetMigrationsTableScript();
        protected internal abstract Task<IEnumerable<Migration>> GetAppliedMigrations(DbConnection? db = null, DbTransaction? tx = null);
        protected internal abstract Task RecordMigration(MigrationScript script, DbConnection db, DbTransaction? tx = null);

        public async Task<IEnumerable<MigrationScript>> GetUnappliedMigrations(DbConnection? db = null)
        {
            var applied = await GetAppliedMigrations();

            var latestApplied = applied.OrderBy(a => a.Version).ThenBy(a => a.Phase).ThenBy(a => a.Number).LastOrDefault();

            return Migrations.Where(s => !applied.Any(a => a.Version == s.Version && a.Phase == s.Phase && a.Number == s.Number));
        }

        public async Task<Version?> GetVersion() => (await GetAppliedMigrations()).LastOrDefault()?.Version;
    }
}
