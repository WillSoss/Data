using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WillSoss.DbDeploy.Sql
{
    public static class Services
    {
        //public static IServiceCollection AddSqlDatabase(this IServiceCollection services, string connectionString, string buildScriptsDirectory, DatabaseOptions? options = null)
        //{
        //    services.AddScoped<Database>(s => new SqlDatabase(connectionString, new ScriptDirectory(buildScriptsDirectory).Scripts, options, s.GetRequiredService<ILogger<SqlDatabase>>()));

        //    return services;
        //}

        //public static IServiceCollection AddAzureSqlDatabase(this IServiceCollection services, string connectionString, string buildScriptsDirectory, DatabaseOptions? options = null)
        //{
        //    services.AddScoped<Database>(s => new AzureSqlDatabase(connectionString, new ScriptDirectory(buildScriptsDirectory).Scripts, options, s.GetRequiredService<ILogger<AzureSqlDatabase>>()));

        //    return services;
        //}
    }
}
