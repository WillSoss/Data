using WillSoss.Data;
using WillSoss.Data.Sql;

await DatabaseCli
    .CreateDefaultBuilder(args)
    .ConfigureDatabase(SqlDatabase
        .CreateBuilder()
        .AddAction("method", db =>
        {
            Console.WriteLine("Action call works");
            return Task.CompletedTask;
        }))
    .Build()
    .RunAsync(CancellationToken.None);