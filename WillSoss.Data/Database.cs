using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.Common;

namespace WillSoss.Data
{
    public abstract class Database
    {
        private readonly ILogger _logger;
        private IEnumerable<Script> _buildScripts;

        public string ConnectionString { get; }
        public Script CreateScript { get; private set; }
        public IEnumerable<Script> BuildScripts => _buildScripts.OrderBy(s => s.Version);
        public Script ResetScript { get; private set; }
        public Script DropScript { get; private set; }

        protected Database(string connectionString, Script create, IEnumerable<Script> build, Script reset, Script drop, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            ConnectionString = connectionString;
            CreateScript = create ?? throw new ArgumentNullException(nameof(create));
            _buildScripts = build ?? throw new ArgumentNullException(nameof(build));
            ResetScript = reset ?? throw new ArgumentNullException(nameof(reset));
            DropScript = drop ?? throw new ArgumentNullException(nameof(drop));
            _logger = logger;
        }

        public virtual async Task Create()
        {
            _logger.LogInformation($"Creating database {GetDatabaseName()}");

            using var db = GetConnectionWithoutDatabase();
            
            await ExecuteScriptAsync(CreateScript, db, replacementTokens: GetTokens());

            _logger?.LogInformation($"Finished creating database {GetDatabaseName()}");
        }

        public virtual async Task Build()
        {
            _logger.LogInformation($"Building database {GetDatabaseName()}");

            using var db = GetConnection();

            await db.EnsureOpenAsync();

            using var tx = db.BeginTransaction();

            await ExecuteScriptsAsync(BuildScripts, db, tx, GetTokens());

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
        }

        public async Task ExecuteScriptsAsync(IEnumerable<Script> scripts, DbConnection db, DbTransaction? tx = null, Dictionary<string, string>? replacementTokens = null)
        {
            foreach (var script in scripts)
                await ExecuteScriptAsync(script, db, tx, replacementTokens);
        }
        public async Task ExecuteScriptAsync(Script script, DbConnection db, DbTransaction? tx = null, Dictionary<string, string>? replacementTokens = null)
        {
            foreach (string batch in script.Batches)
                await ExecuteScriptAsync(batch, db, tx, replacementTokens);
        }

        public async Task ExecuteScriptAsync(string sql, DbConnection db, DbTransaction? tx = null, Dictionary<string, string>? replacementTokens = null)
        {
            foreach (var token in replacementTokens)
                sql = sql.Replace($"{{{{{token.Key}}}}}", token.Value);

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
    }
}
