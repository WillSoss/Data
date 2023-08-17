using Microsoft.Extensions.Logging;
using System.Reflection;

namespace WillSoss.Data.Sql
{
    public class AzureSqlDatabase : SqlDatabase
    {
        private static readonly Script DefaultCreateScript = new Script(DefaultScriptAssembly, DefaultScriptNamespace, "create-az.sql");
        private static readonly Script DefaultDropScript = new Script(DefaultScriptAssembly, DefaultScriptNamespace, "drop-az.sql");

        public static DatabaseBuilder ConnectTo(string connectionString) => 
            new DatabaseBuilder(b => new AzureSqlDatabase(b), connectionString, DefaultCreateScript, DefaultDropScript);

        private AzureSqlDatabase(DatabaseBuilder builder)
            : base(builder) { }
    }
}
