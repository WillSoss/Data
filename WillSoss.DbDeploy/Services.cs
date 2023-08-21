using Microsoft.Extensions.DependencyInjection;

namespace WillSoss.DbDeploy
{
    public static class Services
    {
        public static IServiceCollection AddDatabaseBuilder(this IServiceCollection services, DatabaseBuilder builder)
        {
           services.AddSingleton(s => builder);

            return services;
        }

        public static IServiceCollection AddDatabaseBuilder(this IServiceCollection services, Func<DatabaseBuilder> builder)
        {
            services.AddSingleton(s => builder());

            return services;
        }
    }
}
