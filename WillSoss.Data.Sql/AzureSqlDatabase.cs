using Microsoft.Extensions.Logging;
using System.Reflection;

namespace WillSoss.Data.Sql
{
    public class AzureSqlDatabase : SqlDatabase
    {
        private static readonly Script DefaultCreateScript = new Script(DefaultScriptAssembly, DefaultScriptNamespace, "create-az.sql");
        private static readonly Script DefaultDropScript = new Script(DefaultScriptAssembly, DefaultScriptNamespace, "drop-az.sql");

        public static DatabaseBuilder CreateBuilder() => 
            new DatabaseBuilder(b => new AzureSqlDatabase(b), DefaultCreateScript, DefaultDropScript);

        private AzureSqlDatabase(DatabaseBuilder builder)
            : base(builder) { }
    }
}
