using WillSoss.Data;
using WillSoss.Data.Sql;

await DatabaseCli
    .CreateDefaultBuilder(args)
    .ConfigureDatabase(SqlDatabase.CreateBuilder())
    .Build()
    .RunAsync(CancellationToken.None);