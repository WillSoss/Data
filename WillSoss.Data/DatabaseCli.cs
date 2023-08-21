using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
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
                // Parses the command line and registes the corresponding CliCommand
                GetCommandLineBuilder(services)
                    .UseParseErrorReporting()
                    .Build()
                    .Invoke(args);
            });
        }

        public static IHostBuilder ConfigureDatabase(this IHostBuilder builder, Func<IServiceCollection, DatabaseBuilder> configure)
        {
            builder.ConfigureServices(s => s.AddDatabaseBuilder(configure(s)));
            return builder;
        }
        public static IHostBuilder ConfigureDatabase(this IHostBuilder builder, DatabaseBuilder databaseBuilder)
        {
            builder.ConfigureServices(s => s.AddDatabaseBuilder(databaseBuilder));
            return builder;
        }

        public static async Task RunAsync(this IHost host, CancellationToken cancellationToken)
        {
            var command = host.Services.GetService<ICliCommand>();

            if (command is not null)
                await command.RunAsync(cancellationToken);
        }

        static CommandLineBuilder GetCommandLineBuilder(IServiceCollection services)
        {
            var root = new RootCommand();
            root.AddGlobalOption(CliOptions.ConnectionStringOption);

            root.AddCommand(DeployCommand.Create(services));
            root.AddCommand(CreateCommand.Create(services));
            root.AddCommand(DropCommand.Create(services));
            root.AddCommand(MigrateCommand.Create(services));
            root.AddCommand(ResetCommand.Create(services));
            root.AddCommand(RunCommand.Create(services));

            return new CommandLineBuilder(root);
        }
    }
}
