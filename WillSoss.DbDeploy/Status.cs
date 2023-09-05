namespace WillSoss.DbDeploy
{
    public class Status
    {
        public string Host { get; } = string.Empty;
        public string Database { get; } = string.Empty;
        public IEnumerable<MigrationScript> Applied { get; init; } = Enumerable.Empty<MigrationScript>();
        public IEnumerable<MigrationScript> ToApply { get; init; } = Enumerable.Empty<MigrationScript>();
        public IEnumerable<MigrationScript> Unapplied { get; init; } = Enumerable.Empty<MigrationScript>();
    }
}
