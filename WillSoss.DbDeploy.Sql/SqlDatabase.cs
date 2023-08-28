﻿using Dapper;
using Microsoft.Data.SqlClient;
using System.Data.Common;
using System.Reflection;

namespace WillSoss.DbDeploy.Sql
{
    public class SqlDatabase : Database
	{
		internal static readonly Assembly DefaultScriptAssembly = typeof(SqlDatabase).Assembly;
		internal static readonly string DefaultScriptNamespace = typeof(SqlDatabase).Namespace!;

		private static readonly Script OnPremiseCreateScript = new Script(DefaultScriptAssembly, DefaultScriptNamespace, "create.sql");
		private static readonly Script OnPremiseDropScript = new Script(DefaultScriptAssembly, DefaultScriptNamespace, "drop.sql");

        private static readonly Script AzureCreateScript = new(DefaultScriptAssembly, DefaultScriptNamespace, "create-az.sql");
        private static readonly Script AzureDropScript = new(DefaultScriptAssembly, DefaultScriptNamespace, "drop-az.sql");

		public static DatabaseBuilder CreateBuilder() =>
			new DatabaseBuilder(b =>
				new SqlDatabase(b),
				async db => (await ((SqlDatabase)db).IsAzure()) ? AzureCreateScript : OnPremiseCreateScript,
				async db => (await ((SqlDatabase)db).IsAzure()) ? AzureCreateScript : OnPremiseCreateScript);

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

        Task<bool> IsAzure(DbConnection? db = null) => (db ?? GetConnectionWithoutDatabase()).ExecuteScalarAsync<bool>("select iif(serverproperty('edition') = N'SQL Azure', 1, 0);", commandTimeout: CommandTimeout);


        protected internal override string GetDatabaseName() => new SqlConnectionStringBuilder(ConnectionString).InitialCatalog;

        protected internal override string GetServerName() => new SqlConnectionStringBuilder(ConnectionString).DataSource;

        protected internal override Script GetMigrationsTableScript() => new Script(DefaultScriptAssembly, DefaultScriptNamespace, "build-migration-table.sql");

        protected internal override async Task RecordMigration(Script script, DbConnection db, DbTransaction? tx = null)
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

        protected internal override async Task<IEnumerable<Migration>> GetAppliedMigrations(DbConnection db, DbTransaction? tx = null) =>
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