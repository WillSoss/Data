using Microsoft.Data.SqlClient;

namespace WillSoss.Data.Sql
{
    public class SqlExceptionWithSource : Exception
    {
        readonly SqlException ex;
        readonly string sql;

        public SqlExceptionWithSource(SqlException ex, string sql)
        {
            this.ex = ex;
            this.sql = sql;
        }

        public override string ToString()
        {
            return ex.ToString() + "\n\nSQL:\n" + sql;
        }
    }
}
