namespace WillSoss.DbDeploy
{
    public class SqlExceptionWithSource : Exception
    {
        public string Sql { get; }

        public SqlExceptionWithSource(Exception ex, string sql)
            : base($"{ex.Message} SQL:\n{sql}", ex)
        {
            Sql = sql;
        }
    }
}
