using Microsoft.Extensions.Logging;
using System.Reflection;

namespace WillSoss.Data.Sql
{
    public class AzureSqlDatabase : SqlDatabase
    {
        private static readonly Script DefaultCreateScript = new(DefaultScriptAssembly, DefaultScriptNamespace, "create-az.sql");
        private static readonly Script DefaultDropScript = new(DefaultScriptAssembly, DefaultScriptNamespace, "drop-az.sql");

        public static new DatabaseBuilder CreateBuilder() => 
            new(b => new AzureSqlDatabase(b), DefaultCreateScript, DefaultDropScript);

        private AzureSqlDatabase(DatabaseBuilder builder)
            : base(builder) { }
    }
}
