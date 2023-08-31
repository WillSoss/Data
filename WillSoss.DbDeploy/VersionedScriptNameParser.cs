using System.Text.RegularExpressions;

namespace WillSoss.DbDeploy
{
    internal class Parser
    {
        //https://regex101.com/r/Lky8mU/1
        private static readonly Regex scriptPattern = new Regex(@"^(?<version>\d+(\.\d+){0,3})[-_\s]+(?<name>[-_\s\w]+)*\.sql$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // https://regex101.com/r/Udi5nA/1
        private static readonly Regex filePattern = new Regex(@"^(?<number>\d+)([-_ ]+(?<name>[- \w]+)?)?\.sql$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // https://regex101.com/r/1gu1g5/1
        private readonly static Regex folderPattern = new Regex(@"^v?(?<version>\d+(\.\d+){0,3})([-_ ]+(?<name>[- \w\.]+)?)?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        internal static (string version, string name) ParseFileName(string file)
        {
            string? version = null;
            string? name = null;

            if (!TryParseFileName(file, out version, out name))
                throw new InvalidScriptNameException(file, "Scripts must be named in the format '#[.#[.#[.#]]]-name.sql'");

            return (version!, name!);
        }

        internal static bool TryParseFileName(string file, out string? version, out string? name)
        {
            var match = scriptPattern.Match(Path.GetFileName(file));

            if (!match.Success)
            {
                version = null;
                name = null;

                return false;
            }
            else
            {
                version = match.Groups["version"].Captures[0].Value;
                name = match.Groups["name"].Captures[0].Value;

                return true;
            }
        }

        internal static (string version, string name) ParseFolderName(string file)
        {
            string? version = null;
            string? name = null;

            if (!TryParseFolderName(file, out version, out name))
                throw new InvalidScriptNameException(file, "Scripts must be named in the format '#[.#[.#[.#]]]-name.sql'");

            return (version!, name!);
        }

        internal static bool TryParseFolderName(string file, out string? version, out string? name)
        {
            var match = scriptPattern.Match(Path.GetFileName(file));

            if (!match.Success)
            {
                version = null;
                name = null;

                return false;
            }
            else
            {
                version = match.Groups["version"].Captures[0].Value;
                name = match.Groups["name"].Captures[0].Value;

                return true;
            }
        }

    }
}
