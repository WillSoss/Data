using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.Common;

namespace WillSoss.Data.Sql
{
    public class SqlDatabase
	{
		readonly string _connectionString;
		readonly int? _commandTimeout;
		readonly ILogger<SqlDatabase>? _logger;

		/// <summary>
		/// List of create scripts to run in order
		/// </summary>
		public List<Script> CreateScripts { get; } = new List<Script>();
		/// <summary>
		/// List of clear scripts to run in order
		/// </summary>
		public List<Script> ClearScripts { get; } = new List<Script>();

		public string ConnectionString => _connectionString;

		public SqlDatabase(string connectionString, int? commandTimeout = null, ILogger<SqlDatabase>? logger = null)
		{
			if (string.IsNullOrWhiteSpace(connectionString))
				throw new ArgumentNullException(nameof(connectionString));

			_connectionString = connectionString;
			_commandTimeout = commandTimeout;
			_logger = logger;
		}

		async Task<bool> ExistsAsync()
		{
			var builder = new SqlConnectionStringBuilder(_connectionString);

			string database = builder.InitialCatalog;
			builder.InitialCatalog = "";

			using var conn = new SqlConnection(builder.ToString());

			var exists = await conn.ExecuteScalarAsync<bool>("SELECT TOP 1 1 FROM sys.sysdatabases WHERE [Name] = @database", new { database }, commandTimeout: _commandTimeout);

            _logger.LogInformation($"Database '{database}'{(exists ? " exists" : " does not exist")}.");

			return exists;
		}

		public async Task RebuildAsync()
		{
			await DropAsync();
			await BuildAsync();
		}

		public async Task BuildAsync()
		{
			var builder = new SqlConnectionStringBuilder(_connectionString);

			string database = builder.InitialCatalog;
			builder.InitialCatalog = "";

			bool isAzure = false;
			using (var conn = new SqlConnection(builder.ToString()))
			{
				isAzure = await IsAzure(conn);

				_logger?.LogInformation($"Creating database '{database}'{(isAzure ? " on azure" : "")}.");

				if (isAzure)
				{
					await conn.ExecuteAsync($"IF NOT EXISTS (SELECT 1 FROM sys.sysdatabases WHERE name = '{database}') CREATE DATABASE [{database}] ( EDITION = 'basic')", commandTimeout: _commandTimeout);
				}
				else
				{
					await conn.ExecuteAsync($"IF NOT EXISTS (SELECT 1 FROM sys.sysdatabases WHERE name = '{database}') CREATE DATABASE [{database}]", commandTimeout: _commandTimeout);
				}
			}

            _logger?.LogInformation($"Finished creating database '{database}'{(isAzure ? " on azure" : "")}.");

            await WaitForAzureDeployment(isAzure);

			using (var db = new SqlConnection(_connectionString))
			{
				await BuildSchemaAsync(db);
			}
		}

		public async Task BuildSchemaAsync(DbConnection db)
		{
			await db.EnsureOpenAsync();

			_logger?.LogInformation($"Running create scripts.");

			await ExecuteScriptsAsync(CreateScripts, db);

            _logger?.LogInformation($"Finished running create scripts.");
        }

		public async Task ClearAsync()
		{
			using (var db = new SqlConnection(_connectionString))
			{
				await ClearAsync(db);
			}
		}

		public async Task ClearAsync(DbConnection db)
		{
			await db.EnsureOpenAsync();

			_logger?.LogInformation($"Clearing database.");

			using (var tx = db.BeginTransaction())
			{
				await ExecuteScriptsAsync(ClearScripts, db, tx);

				tx.Commit();
			}

			_logger?.LogInformation($"Finished clearing database.");
		}

		public async Task DropAsync()
		{
			var builder = new SqlConnectionStringBuilder(_connectionString);

			string database = builder.InitialCatalog;
			builder.InitialCatalog = "";

			bool isAzure;
			using (var conn = new SqlConnection(builder.ToString()))
			{
				isAzure = await IsAzure(conn);

				_logger?.LogInformation($"Dropping database '{database}'{(isAzure ? " on azure" : "")}.");

				if (isAzure)
				{
					await conn.ExecuteAsync($@"DROP DATABASE IF EXISTS [{database}]", commandTimeout: _commandTimeout);
				}
				else
				{
					await conn.ExecuteAsync($@"
						IF EXISTS (SELECT 1 FROM sys.sysdatabases WHERE name = '{database}')
						BEGIN
							ALTER DATABASE [{database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
							DROP DATABASE [{database}];
						END", commandTimeout: _commandTimeout);
				}

				_logger?.LogInformation("Finished dropping database.");
			}

			await WaitForAzureDeployment(isAzure);
		}

		async Task ExecuteScriptsAsync(IEnumerable<Script> scripts, IDbConnection db, IDbTransaction? tx = null)
		{
			foreach (var script in scripts)
				foreach (string batch in script.Batches)
					await ExecuteScriptAsync(batch, db, tx);
		}

		async Task ExecuteScriptAsync(string sql, IDbConnection db, IDbTransaction? tx = null)
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

		Task<bool> IsAzure(DbConnection conn) => conn.ExecuteScalarAsync<bool>("SELECT CASE WHEN SERVERPROPERTY ('edition') = N'SQL Azure' THEN 1 END", commandTimeout: _commandTimeout);

		async Task WaitForAzureDeployment(bool isAzure)
		{
			if (isAzure)
			{
				_logger?.LogInformation("Waiting 90 seconds for azure resource deployment.");

				await Task.Delay(3000);
			}
		}
	}
}