using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WillSoss.Data.Sql
{
    public static class Services
    {
        public static IServiceCollection AddSqlDatabase(this IServiceCollection services, string connectionString, string buildScriptsDirectory, DatabaseOptions? options)
        {
            services.AddScoped<Database>(s => new SqlDatabase(connectionString, new ScriptDirectory(buildScriptsDirectory).Scripts, options, s.GetRequiredService<ILogger<SqlDatabase>>()));

            return services;
        }
    }
}
