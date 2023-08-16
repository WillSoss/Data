using Microsoft.Extensions.Logging;
using System.Reflection;

namespace WillSoss.Data.Sql
{
    public class AzureSqlDatabase : SqlDatabase
    {
        private static readonly Assembly DefaultScriptAssembly = typeof(SqlDatabase).Assembly;
        private static readonly string DefaultScriptNamespace = typeof(SqlDatabase).Namespace!;

        public static readonly Script AzureCreateScript = new Script(DefaultScriptAssembly, DefaultScriptNamespace, "create-az.sql");
        public static readonly Script AzureDropScript = new Script(DefaultScriptAssembly, DefaultScriptNamespace, "drop-az.sql");

        public AzureSqlDatabase(string connectionString, IEnumerable<Script> build, DatabaseOptions? options, ILogger<SqlDatabase> logger)
            : base(connectionString, build, new DatabaseOptions()
            {
                CreateScript = options?.CreateScript ?? AzureCreateScript,
                ResetScript = options?.ResetScript ?? DefaultResetScript,
                DropScript = options?.DropScript ?? AzureDropScript,
                CommandTimeout = options?.CommandTimeout,
                PostCreateDelay = options?.PostCreateDelay,
                PostDropDelay = options?.PostDropDelay
            }, logger) { }
    }
}
