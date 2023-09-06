namespace WillSoss.DbDeploy
{
    public class MissingMigrationsException : Exception
    {
        public IEnumerable<MigrationScript> MissingScripts { get; }

        public MissingMigrationsException(IEnumerable<MigrationScript> missing)
            : base($"One or more migrations have not been applied in the correct order.") 
        {
            MissingScripts = missing;
        }
    }
}
