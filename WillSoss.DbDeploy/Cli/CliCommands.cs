using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;

namespace WillSoss.DbDeploy.Cli
{
    internal static class CliCommands
    {
        internal static Command Drop(IServiceCollection services)
        {
            var command = new Command("drop", "Drops the database if it exists."); ;

            command.AddOption(CliOptions.Unsafe);

            command.SetHandler((cs, @unsafe) => services.AddTransient<ICliCommand>(s => new DeployCommand(
                s.GetRequiredService<DatabaseBuilder>(),
                cs,
                null,
                true,
                @unsafe,
                false,
                false,
                false,
                false
                )), CliOptions.ConnectionString, CliOptions.Unsafe);

            return command;
        }

        internal static Command Create(IServiceCollection services)
        {
            var command = new Command("create", "Creates the database if it does not exist."); ;

            command.AddOption(CliOptions.Drop);
            command.AddOption(CliOptions.Unsafe);

            command.SetHandler((cs, drop, @unsafe) => services.AddTransient<ICliCommand>(s => new DeployCommand(
                s.GetRequiredService<DatabaseBuilder>(),
                cs,
                null,
                drop,
                @unsafe,
                true,
                false,
                false,
                false
                )), CliOptions.ConnectionString, CliOptions.Drop, CliOptions.Unsafe);

            return command;
        }

        internal static Command Migrate(IServiceCollection services)
        {
            var command = new Command("migrate", "Migrates to the specified version, or latest if no version is specified."); ;

            command.AddOption(CliOptions.Version);
            command.AddOption(CliOptions.Pre);
            command.AddOption(CliOptions.Post);

            command.SetHandler((cs, version, pre, post) => services.AddTransient<ICliCommand>(s => new DeployCommand(
                s.GetRequiredService<DatabaseBuilder>(),
                cs,
                version,
                false,
                false,
                false,
                true,
                pre,
                post
                )), CliOptions.ConnectionString, CliOptions.Version, CliOptions.Pre, CliOptions.Post);

            return command;
        }

        internal static Command Deploy(IServiceCollection services)
        {
            var command = new Command("deploy", "Creates the database if it does not exist, then migrates to latest."); ;

            command.AddOption(CliOptions.Version);
            command.AddOption(CliOptions.Drop);
            command.AddOption(CliOptions.Unsafe);
            command.AddOption(CliOptions.Pre);
            command.AddOption(CliOptions.Post);

            command.SetHandler((cs, version, drop, @unsafe, pre, post) => services.AddTransient<ICliCommand>(s => new DeployCommand(
                s.GetRequiredService<DatabaseBuilder>(),
                cs,
                version,
                drop,
                @unsafe,
                true,
                true,
                pre,
                post
                )), CliOptions.ConnectionString, CliOptions.Version, CliOptions.Drop, CliOptions.Unsafe, CliOptions.Pre, CliOptions.Post);

            return command;
        }

        internal static Command Status(IServiceCollection services)
        {
            var command = new Command("status", "Displays the migration status of the database.");

            command.SetHandler((cs) => services.AddTransient<ICliCommand>(s => new StatusCommand(
                s.GetRequiredService<DatabaseBuilder>(),
                cs
                )), CliOptions.ConnectionString);

            return command;
        }

        internal static Command Run(IServiceCollection services)
        {
            var command = new Command("run", "Executes a script or action."); ;

            var arg = new Argument<string?>("script", "The script or action to run.")
            {
                Arity = ArgumentArity.ExactlyOne
            };

            command.AddArgument(arg);

            command.SetHandler((cs, action) => services.AddTransient<ICliCommand>(s => new RunCommand(
                s.GetRequiredService<DatabaseBuilder>(),
                cs,
                action
                )), CliOptions.ConnectionString, arg);

            return command;
        }

        internal static Command Reset(IServiceCollection services)
        {
            var command = new Command("reset", "Runs the reset script on the database. Can be used to clean up data after test runs."); ;

            command.AddOption(CliOptions.Unsafe);

            command.SetHandler((cs, @unsafe) => services.AddTransient<ICliCommand>(s => new ResetCommand(
                s.GetRequiredService<DatabaseBuilder>(),
                cs,
                @unsafe
                )), CliOptions.ConnectionString, CliOptions.Unsafe);

            return command;
        }
    }
}
