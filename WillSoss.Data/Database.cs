using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace WillSoss.Data
{
    public abstract class Database
    {
        private IEnumerable<Script> _buildScripts;
        private readonly int _commandTimeout;
        private readonly int _postCreateDelay;
        private readonly int _postDropDelay;
        private readonly ILogger _logger;

        public string ConnectionString { get; }
        public int CommandTimeout => _commandTimeout;
        public Script CreateScript { get; private set; }
        public IEnumerable<Script> BuildScripts => _buildScripts.OrderBy(s => s.Version);
        public Script ResetScript { get; private set; }
        public Script DropScript { get; private set; }

        protected Database(string connectionString, IEnumerable<Script> buildScripts, DatabaseOptions options, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            ConnectionString = connectionString;
            CreateScript = options?.CreateScript ?? throw new ArgumentNullException(nameof(options.CreateScript));
            _buildScripts = buildScripts ?? throw new ArgumentNullException(nameof(buildScripts));
            ResetScript = options?.ResetScript ?? throw new ArgumentNullException(nameof(options.ResetScript));
            DropScript = options?.DropScript ?? throw new ArgumentNullException(nameof(options.DropScript));
            _commandTimeout = options?.CommandTimeout ?? 90;
            _postCreateDelay = options?.PostCreateDelay ?? 0;
            _postDropDelay = options?.PostDropDelay ?? 0;
            _logger = logger;
        }

        public virtual async Task Create()
        {
            _logger.LogInformation($"Creating database {GetDatabaseName()}");

            using (var db = GetConnectionWithoutDatabase())
            {
                await ExecuteScriptAsync(CreateScript, db, replacementTokens: GetTokens());
            }

            _logger?.LogInformation($"Finished creating database {GetDatabaseName()}");

            if (_postCreateDelay > 0)
            {
                _logger?.LogInformation($"Waiting {_postCreateDelay} seconds for resource deployment...");
                await Task.Delay(TimeSpan.FromSeconds(_postCreateDelay));
            }

            _logger.LogInformation($"Adding migration schema to database {GetDatabaseName()}");

            using (var db = GetConnection())
            {
                await ExecuteScriptAsync(GetMigrationsTableScript(), db, replacementTokens: GetTokens());
            }
        }

        /// <summary>
        /// Builds the database using the <see cref="BuildScripts"/>.
        /// </summary>
        /// <param name="version">Applies builds scripts up to the specified version.</param>
        public virtual async Task Build(Version? version = null)
        {
            _logger.LogInformation($"Building database {GetDatabaseName()}");

            using var db = GetConnection();

            await db.EnsureOpenAsync();

            using var tx = db.BeginTransaction();

            var applied = (await GetAppliedMigrations(db, tx)).Select(m => m.Version);

            var scriptsToApply = BuildScripts.Where(s => !applied.Contains(s.Version));

            if (version is not null)
                scriptsToApply = scriptsToApply.Where(s => s.Version <= version);

            await ExecuteScriptsAsync(scriptsToApply, db, tx, GetTokens(), true);

            tx.Commit();

            _logger?.LogInformation($"Finished creating database {GetDatabaseName()}");
        }

        public virtual async Task Reset()
        {
            _logger.LogInformation($"Resetting data in database {GetDatabaseName()}");

            using var db = GetConnectionWithoutDatabase();

            await db.EnsureOpenAsync();

            using var tx = db.BeginTransaction();

            await ExecuteScriptAsync(ResetScript, db, tx, GetTokens());

            _logger?.LogInformation($"Finished resetting data in database {GetDatabaseName()}");
        }

        public virtual async Task Drop()
        {
            _logger.LogInformation($"Dropping database {GetDatabaseName()}");

            using var db = GetConnectionWithoutDatabase();

            await ExecuteScriptAsync(DropScript, db, replacementTokens: GetTokens());

            _logger?.LogInformation($"Finished dropping database {GetDatabaseName()}");

            if (_postDropDelay > 0)
            {
                _logger?.LogInformation($"Waiting {_postDropDelay} seconds for resource deployment...");
                await Task.Delay(TimeSpan.FromSeconds(_postDropDelay));
            }
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

        protected Dictionary<string, string> GetTokens() => new Dictionary<string, string>()
        {
            { "database", GetDatabaseName() }
        };

        protected abstract DbConnection GetConnection();
        protected abstract DbConnection GetConnectionWithoutDatabase();
        protected abstract string GetDatabaseName();
        protected abstract Script GetMigrationsTableScript();
        protected abstract Task<IEnumerable<Migration>> GetAppliedMigrations(DbConnection db, DbTransaction? tx = null);
        protected abstract Task RecordMigration(Script script, DbConnection db, DbTransaction? tx = null);
    }
}
