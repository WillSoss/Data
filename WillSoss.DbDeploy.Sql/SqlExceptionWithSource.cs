using Microsoft.Data.SqlClient;

namespace WillSoss.DbDeploy.Sql
{
    public class SqlExceptionWithSource : Exception
    {
        public string Sql { get; }

        public SqlExceptionWithSource(SqlException ex, string sql)
            : base($"{ex.Message} SQL:\n{sql}", ex)
        {
            Sql = sql;
        }
    }
}
