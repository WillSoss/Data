namespace WillSoss.DbDeploy
{
    public class UnableToMigrateException : Exception
    {
        public UnableToMigrateException(string message)
            : base(message) { }
    }
}
