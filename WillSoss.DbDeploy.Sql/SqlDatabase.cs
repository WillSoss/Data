using Dapper;
using Microsoft.Data.SqlClient;
using System.Data.Common;
using System.Reflection;

namespace WillSoss.DbDeploy.Sql
{
    public class SqlDatabase : Database
	{
		internal static readonly Assembly DefaultScriptAssembly = typeof(SqlDatabase).Assembly;
		internal static readonly string DefaultScriptNamespace = typeof(SqlDatabase).Namespace!;

		public static readonly Script OnPremiseCreateScript = new Script(DefaultScriptAssembly, DefaultScriptNamespace, "create.sql");
		public static readonly Script OnPremiseDropScript = new Script(DefaultScriptAssembly, DefaultScriptNamespace, "drop.sql");

        public static readonly Script AzureCreateScript = new(DefaultScriptAssembly, DefaultScriptNamespace, "create-az.sql");
        public static readonly Script AzureDropScript = new(DefaultScriptAssembly, DefaultScriptNamespace, "drop-az.sql");

		public static DatabaseBuilder CreateBuilder() =>
			new DatabaseBuilder(b =>
				new SqlDatabase(b),
				async db => (await ((SqlDatabase)db).IsAzure()) ? AzureCreateScript : OnPremiseCreateScript,
				async db => (await ((SqlDatabase)db).IsAzure()) ? AzureDropScript : OnPremiseDropScript);

        protected SqlDatabase(DatabaseBuilder builder)
			: base(builder) 
		{
			SqlMapper.AddTypeHandler(new VersionTypeMapper());
        }

		protected internal override DbConnection GetConnection() => new SqlConnection(ConnectionString);

        protected internal override DbConnection GetConnectionWithoutDatabase()
        {
            var builder = new SqlConnectionStringBuilder(ConnectionString);

            builder.InitialCatalog = "";

            return new SqlConnection(builder.ToString());
        }

        public Task<bool> IsAzure(DbConnection? db = null) => (db ?? GetConnectionWithoutDatabase()).ExecuteScalarAsync<bool>("select iif(serverproperty('edition') = N'SQL Azure', 1, 0);", commandTimeout: CommandTimeout);

        protected internal override string GetDatabaseName() => new SqlConnectionStringBuilder(ConnectionString).InitialCatalog;

        protected internal override string GetServerName() => new SqlConnectionStringBuilder(ConnectionString).DataSource;

        protected internal override Script GetMigrationsTableScript() => new Script(DefaultScriptAssembly, DefaultScriptNamespace, "build-migration-table.sql");

        protected internal override async Task RecordMigration(MigrationScript script, DbConnection db, DbTransaction? tx = null)
        {
			await db.ExecuteAsync("insert into cfg.migration (major, minor, build, rev, phase, number, [description]) values (@major, @minor, @build, @rev, @phase, @number, @desc);", new
			{
				major = script.Version.Major,
				minor = script.Version.Minor,
				build = script.Version.Build,
				rev = script.Version.Revision,
				phase = script.Phase,
				number = script.Number,
				desc = script.Name
			}, tx, CommandTimeout);
        }

        protected internal override async Task<IEnumerable<Migration>> GetAppliedMigrations(DbConnection? db = null, DbTransaction? tx = null)
		{
			if (await Exists())
			{
				try
				{
					return await (db ?? GetConnection()).QueryAsync<Migration>(@"select * from cfg.migration_detail;", new { }, tx, CommandTimeout);
				}
				catch
				{
					return Enumerable.Empty<Migration>();
				}
            }
			else
				return Enumerable.Empty<Migration>();
        }
			

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

        public override async Task<bool> Exists()
        {
			using var db = GetConnectionWithoutDatabase();

			var exists = await db.ExecuteScalarAsync<int?>("select 1 from sys.sysdatabases where name = @database;", new { database = GetDatabaseName() });

			return exists == 1;
        }
    }
}