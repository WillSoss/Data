namespace WillSoss.DbDeploy
{
    public class MigrationsNotAppliedInOrderException : Exception
    {
        public MigrationsNotAppliedInOrderException(Migration dbVersion, MigrationScript scriptVersion)
            : base($"Cannot apply script with {scriptVersion} to datbase at version {dbVersion}. Migrations have not been applied in order.") { }
    }
}
