using WillSoss.DbDeploy;
using WillSoss.DbDeploy.Sql;

await DatabaseCli
    .CreateDefaultBuilder(args)
    .ConfigureDatabase(SqlDatabase
        .CreateBuilder()
        .AddMigrations(Path.Combine(Directory.GetCurrentDirectory(), "Migrations"))
        .AddAction("method", db =>
        {
            Console.WriteLine("Action call works");
            return Task.CompletedTask;
        }))
    .Build()
    .RunAsync(CancellationToken.None);