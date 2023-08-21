namespace WillSoss.DbDeploy
{
    public class Migration
    {
        public Version Version { get; set; } = new Version();
        public string Description { get; set; } = string.Empty;
        public DateTime AppliedAt { get; set; }
    }
}
