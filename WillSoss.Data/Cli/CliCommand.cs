using System.CommandLine;

namespace WillSoss.Data.Cli
{
    internal abstract class CliCommand
    {
        internal Task RunAsync() => RunAsync(new CancellationTokenSource().Token);
        internal abstract Task RunAsync(CancellationToken cancel);

        internal static Option<string?> ConnectionStringOption = new Option<string?>(new[] { "--connectionString", "-c" }, 
            description: "Connection string of the database to modify. Optional when a default is supplied by the application.");

        internal static Option<Version?> VersionOption = new Option<Version?>(new[] { "--version", "-v" },
            description: "Optional. Migrates to the specified version instead of latest.",
            parseArgument: result =>
            {
                Version? version;
                if (!Version.TryParse(result.Tokens[0].Value, out version))
                    result.ErrorMessage = "Version must be in the format #[.#[.#[.#]]]";

                return version!.FillZeros();
            })
        {
            Arity = ArgumentArity.ExactlyOne
        };

        internal static Option<bool> DropOption = new Option<bool>(new[] { "--drop", "-d" }, "Optional. Drops the database if it exists, then recreates and migrates.");

        internal static Option<bool> UnsafeOption = new Option<bool>(new[] { "--unsafe" }, "Use with extreme caution. Disables destructive action prevention for production databases using keyword protection.");


    }
}
