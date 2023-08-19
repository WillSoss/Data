using Microsoft.Extensions.DependencyInjection;

namespace WillSoss.Data
{
    public static class Services
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services, Func<Database> configureDatabase)
        {
           services.AddSingleton(s => configureDatabase());

            return services;
        }

    }
}
