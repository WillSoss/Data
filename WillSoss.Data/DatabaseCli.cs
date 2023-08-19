using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine;
using System.CommandLine.Builder;
using WillSoss.Data.Cli;

namespace WillSoss.Data
{
    public static class DatabaseCli
    {
        public static IHostBuilder CreateDefaultBuilder(string[] args)
        {
            return Host
            .CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton(new CliInvoker(args, GetCommandLineBuilder(services)
                    .UseParseErrorReporting()
                    .Build()));
            });
        }

        public static async Task RunAsync(this IHost host, CancellationToken cancellationToken)
        {
            var cli = host.Services.GetRequiredService<CliInvoker>();
            await cli.Invoke();
        }

        static CommandLineBuilder GetCommandLineBuilder(IServiceCollection services)
        {
            var root = new RootCommand();

            root.AddCommand(DeployCommand.Create(services));

            return new CommandLineBuilder(root);
        }
    }
}
