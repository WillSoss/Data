namespace WillSoss.Data
{
    public class DatabaseBuilder
    {
        readonly Func<DatabaseBuilder, Database> _build;
        public DatabaseBuilder(Func<DatabaseBuilder, Database> build) 
        {
            _build = build;
        }

        public DatabaseBuilder AddThing() => this;
    }
}
