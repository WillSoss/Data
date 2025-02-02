using Microsoft.Extensions.Configuration;
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
            .ConfigureServices((context, services) =>
            {
                // Parses the command line and registes the corresponding CliCommand
                GetCommandLineBuilder(context, services)
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

        static CommandLineBuilder GetCommandLineBuilder(HostBuilderContext context, IServiceCollection services)
        {
            var root = new RootCommand();
            root.AddGlobalOption(CliOptions.ConnectionString);

            root.AddCommand(CliCommands.Status(context, services));
            root.AddCommand(CliCommands.Drop(context, services));
            root.AddCommand(CliCommands.Create(context, services));
            root.AddCommand(CliCommands.Migrate(context, services));
            root.AddCommand(CliCommands.Deploy(context, services));
            root.AddCommand(CliCommands.Reset(context, services));
            root.AddCommand(CliCommands.Run(context, services));

            return new CommandLineBuilder(root);
        }
    }
}
