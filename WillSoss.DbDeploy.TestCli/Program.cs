using WillSoss.DbDeploy;
using WillSoss.DbDeploy.Sql;

await DatabaseCli
    .CreateDefaultBuilder(args)
    .ConfigureDatabase(SqlDatabase
        .CreateBuilder()
        //.WithConnectionString("server=.;database=cli-prod;integrated security=true;trust server certificate=true;")
        .WithConnectionStringName("database")
        .AddMigrations(Path.Combine(Directory.GetCurrentDirectory(), "Migrations"))
        .AddAction("method", db =>
        {
            return Task.CompletedTask;
        }))
    .Build()
    .RunAsync(CancellationToken.None);