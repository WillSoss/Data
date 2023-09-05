using System.CommandLine;

namespace WillSoss.DbDeploy.Cli
{
    internal static class CliOptions
    {
        internal static Option<string?> ConnectionString = new Option<string?>(new[] { "--connectionstring", "-c" },
            description: "Connection string of the database to modify. Optional when a default is supplied by the application.");

        internal static Option<Version?> Version = new Option<Version?>(new[] { "--version", "-v" },
            description: "Optional. Migrates to the specified version instead of latest.",
            parseArgument: result =>
            {
                Version? version;
                if (!System.Version.TryParse(result.Tokens[0].Value, out version))
                    result.ErrorMessage = "Version must be in the format #[.#[.#[.#]]]";

                return version!.FillZeros();
            })
        {
            Arity = ArgumentArity.ExactlyOne
        };

        internal static Option<bool> Drop = new Option<bool>(new[] { "--drop" }, "Optional. Drops the database if it exists, then recreates and migrates.");

        internal static Option<bool> Unsafe = new Option<bool>(new[] { "--unsafe" }, "Use with extreme caution. Disables destructive action prevention for production databases using keyword protection.");

        internal static Option<bool> Pre = new Option<bool>(new[] { "--pre" }, "Optional. Limits migrations to pre-deployment migration scripts.");

        internal static Option<bool> Post = new Option<bool>(new[] { "--post" }, "Optional. Limits migrations to post-deployment migration scripts.");
    }
}
