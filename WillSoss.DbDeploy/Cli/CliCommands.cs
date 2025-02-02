using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine;
using System.Xml.Linq;

namespace WillSoss.DbDeploy.Cli
{
    internal static class CliCommands
    {
        internal static Command Drop(HostBuilderContext context, IServiceCollection services)
        {
            var command = new Command("drop", "Drops the database if it exists."); ;

            command.AddOption(CliOptions.Unsafe);

            command.SetHandler((cs, name, @unsafe) => services.AddTransient<ICliCommand>(s => new DeployCommand(
                s.GetRequiredService<DatabaseBuilder>(),
                cs ?? GetConnectionString(context, name),
                null,
                true,
                @unsafe,
                false,
                false,
                false,
                false,
                false
                )), CliOptions.ConnectionString, CliOptions.ConnectionStringName, CliOptions.Unsafe);

            return command;
        }

        internal static Command Create(HostBuilderContext context, IServiceCollection services)
        {
            var command = new Command("create", "Creates the database if it does not exist."); ;

            command.AddOption(CliOptions.Drop);
            command.AddOption(CliOptions.Unsafe);

            command.SetHandler((cs, name, drop, @unsafe) => services.AddTransient<ICliCommand>(s => new DeployCommand(
                s.GetRequiredService<DatabaseBuilder>(),
                cs ?? GetConnectionString(context, name),
                null,
                drop,
                @unsafe,
                true,
                false,
                false,
                false,
                false
                )), CliOptions.ConnectionString, CliOptions.ConnectionStringName, CliOptions.Drop, CliOptions.Unsafe);

            return command;
        }

        internal static Command Migrate(HostBuilderContext context, IServiceCollection services)
        {
            var command = new Command("migrate", "Migrates to the specified version, or latest if no version is specified."); ;

            command.AddOption(CliOptions.Version);
            command.AddOption(CliOptions.ApplyMissing);
            command.AddOption(CliOptions.Pre);
            command.AddOption(CliOptions.Post);

            command.SetHandler((cs, name, version, applyMissing, pre, post) => services.AddTransient<ICliCommand>(s => new DeployCommand(
                s.GetRequiredService<DatabaseBuilder>(),
                cs ?? GetConnectionString(context, name),
                version,
                false,
                false,
                false,
                true,
                applyMissing,
                pre,
                post
                )), CliOptions.ConnectionString, CliOptions.ConnectionStringName, CliOptions.Version, CliOptions.ApplyMissing, CliOptions.Pre, CliOptions.Post);

            return command;
        }

        internal static Command Deploy(HostBuilderContext context, IServiceCollection services)
        {
            var command = new Command("deploy", "Creates the database if it does not exist, then migrates to latest."); ;

            command.AddOption(CliOptions.Version);
            command.AddOption(CliOptions.Drop);
            command.AddOption(CliOptions.Unsafe);
            command.AddOption(CliOptions.ApplyMissing);
            command.AddOption(CliOptions.Pre);
            command.AddOption(CliOptions.Post);

            command.SetHandler((cs, name, version, drop, @unsafe, applyMissing, pre, post) => services.AddTransient<ICliCommand>(s => new DeployCommand(
                s.GetRequiredService<DatabaseBuilder>(),
                cs ?? GetConnectionString(context, name),
                version,
                drop,
                @unsafe,
                true,
                true,
                applyMissing,
                pre,
                post
                )), CliOptions.ConnectionString, CliOptions.ConnectionStringName, CliOptions.Version, CliOptions.Drop, CliOptions.Unsafe, CliOptions.ApplyMissing, CliOptions.Pre, CliOptions.Post);

            return command;
        }

        internal static Command Status(HostBuilderContext context, IServiceCollection services)
        {
            var command = new Command("status", "Displays the migration status of the database.");

            command.SetHandler((cs, name) => services.AddTransient<ICliCommand>(s => new StatusCommand(
                s.GetRequiredService<DatabaseBuilder>(),
                cs ?? GetConnectionString(context, name)
                )), CliOptions.ConnectionString, CliOptions.ConnectionStringName);

            return command;
        }

        internal static Command Run(HostBuilderContext context, IServiceCollection services)
        {
            var command = new Command("run", "Executes a script or action."); ;

            var arg = new Argument<string?>("script", "The script or action to run.")
            {
                Arity = ArgumentArity.ExactlyOne
            };

            command.AddArgument(arg);

            command.SetHandler((cs, name, action) => services.AddTransient<ICliCommand>(s => new RunCommand(
                s.GetRequiredService<DatabaseBuilder>(),
                cs ?? GetConnectionString(context, name),
                action
                )), CliOptions.ConnectionString, CliOptions.ConnectionStringName, arg);

            return command;
        }

        internal static Command Reset(HostBuilderContext context, IServiceCollection services)
        {
            var command = new Command("reset", "Runs the reset script on the database. Can be used to clean up data after test runs."); ;

            command.AddOption(CliOptions.Unsafe);

            command.SetHandler((cs, name, @unsafe) => services.AddTransient<ICliCommand>(s => new ResetCommand(
                s.GetRequiredService<DatabaseBuilder>(),
                cs ?? GetConnectionString(context, name),
                @unsafe
                )), CliOptions.ConnectionString, CliOptions.ConnectionStringName, CliOptions.Unsafe);

            return command;
        }

        private static string? GetConnectionString(HostBuilderContext context, string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return context.Configuration.GetConnectionString(name);
        }
    }
}
