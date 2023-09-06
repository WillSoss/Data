using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using WillSoss.DbDeploy.Cli;

namespace WillSoss.DbDeploy
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
            root.AddGlobalOption(CliOptions.ConnectionString);

            root.AddCommand(CliCommands.Status(services));
            root.AddCommand(CliCommands.Drop(services));
            root.AddCommand(CliCommands.Create(services));
            root.AddCommand(CliCommands.Migrate(services));
            root.AddCommand(CliCommands.Deploy(services));
            root.AddCommand(CliCommands.Reset(services));
            root.AddCommand(CliCommands.Run(services));

            return new CommandLineBuilder(root);
        }
    }
}
