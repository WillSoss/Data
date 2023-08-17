using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Reflection;

namespace WillSoss.Data.Sql
{
    public class SqlDatabase : Database
	{
		internal static readonly Assembly DefaultScriptAssembly = typeof(SqlDatabase).Assembly;
		internal static readonly string DefaultScriptNamespace = typeof(SqlDatabase).Namespace!;

		private static readonly Script DefaultCreateScript = new Script(DefaultScriptAssembly, DefaultScriptNamespace, "create.sql");
		private static readonly Script DefaultDropScript = new Script(DefaultScriptAssembly, DefaultScriptNamespace, "drop.sql");
        public static DatabaseBuilder ConnectTo(string connectionString) =>
			new DatabaseBuilder(b => new SqlDatabase(b), connectionString, DefaultCreateScript, DefaultDropScript);

        protected SqlDatabase(DatabaseBuilder builder)
			: base(builder) { }

		protected override DbConnection GetConnection() => new SqlConnection(ConnectionString);

        protected override DbConnection GetConnectionWithoutDatabase()
        {
            var builder = new SqlConnectionStringBuilder(ConnectionString);

            builder.InitialCatalog = "";

            return new SqlConnection(builder.ToString());
        }

        protected override string GetDatabaseName() => new SqlConnectionStringBuilder(ConnectionString).InitialCatalog;

		protected override Script GetMigrationsTableScript() => new Script(DefaultScriptAssembly, DefaultScriptNamespace, "build-migration-table.sql");

        protected override async Task RecordMigration(Script script, DbConnection db, DbTransaction? tx = null)
        {
			await db.ExecuteAsync("insert into cfg.migration (major, minor, build, rev, [description]) values (@major, @minor, @build, @rev, @desc);", new
			{
				major = script.Version.Major,
				minor = script.Version.Minor,
				build = Math.Max(script.Version.Build, 0),
				rev = Math.Max(script.Version.Revision, 0),
				desc = script.Name
			}, tx, CommandTimeout);
        }

        protected override async Task<IEnumerable<Migration>> GetAppliedMigrations(DbConnection db, DbTransaction? tx = null) =>
			await db.QueryAsync<Migration>(@"select * from cfg.migration_detail;", new { }, tx, CommandTimeout);

        protected override async Task ExecuteScriptAsync(string sql, DbConnection db, DbTransaction? tx = null)
		{
			try
			{
				await db.ExecuteAsync(sql, transaction: tx, commandTimeout: CommandTimeout);
			}
			catch (SqlException ex)
			{
				throw new SqlExceptionWithSource(ex, sql);
			}
		}
	}
}