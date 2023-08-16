using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.Common;
using System.Reflection;

namespace WillSoss.Data.Sql
{
    public class SqlDatabase : Database
	{
		private static readonly Assembly DefaultScriptAssembly = typeof(SqlDatabase).Assembly;
		private static readonly string DefaultScriptNamespace = typeof(SqlDatabase).Namespace!;

		public static readonly Script DefaultCreateScript = new Script(DefaultScriptAssembly, DefaultScriptNamespace, "create.sql");
		public static readonly Script DefaultResetScript = new Script(DefaultScriptAssembly, DefaultScriptNamespace, "reset.sql");
		public static readonly Script DefaultDropScript = new Script(DefaultScriptAssembly, DefaultScriptNamespace, "drop.sql");


		readonly int? _commandTimeout;
		readonly ILogger<SqlDatabase>? _logger;

		public SqlDatabase(string connectionString, IEnumerable<Script> build, Script? create = null, Script? reset = null, Script? drop = null, int? commandTimeout = null, ILogger<SqlDatabase> logger = null)
			: base(connectionString, create ?? DefaultCreateScript, build, reset ?? DefaultResetScript, drop ?? DefaultDropScript, logger)

		{
			_commandTimeout = commandTimeout;
			_logger = logger;
		}

		protected override DbConnection GetConnection() => new SqlConnection(ConnectionString);

        protected override DbConnection GetConnectionWithoutDatabase()
        {
            var builder = new SqlConnectionStringBuilder(ConnectionString);

            builder.InitialCatalog = "";

            return new SqlConnection(builder.ToString());
        }

        protected override string GetDatabaseName() => new SqlConnectionStringBuilder(ConnectionString).InitialCatalog;

		protected override async Task ExecuteScriptAsync(string sql, DbConnection db, DbTransaction? tx = null)
		{
			try
			{
				await db.ExecuteAsync(sql, transaction: tx, commandTimeout: _commandTimeout);
			}
			catch (SqlException ex)
			{
				throw new SqlExceptionWithSource(ex, sql);
			}
		}
	}
}