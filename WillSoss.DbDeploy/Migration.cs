namespace WillSoss.DbDeploy
{
    public enum MigrationPhase
    {
        Pre = 0, 
        Post = 1
    }

    public class Migration
    {
        public Version Version { get; set; } = new Version();
        public MigrationPhase Phase { get; set; }
        public int Number { get; set; }

        public string Description { get; set; } = string.Empty;
        public DateTime AppliedAt { get; set; }

        public override string ToString() => $"{Version}/{Phase}/{Number}";
    }
}
