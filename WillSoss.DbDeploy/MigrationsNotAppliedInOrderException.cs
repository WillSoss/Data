namespace WillSoss.DbDeploy
{
    public class MigrationsNotAppliedInOrderException : Exception
    {
        public MigrationsNotAppliedInOrderException(Version dbVersion, Version scriptVersion)
            : base($"Cannot run script with version {scriptVersion} to datbase at version {dbVersion}. Migrations have not been applied in order.") { }
    }
}
