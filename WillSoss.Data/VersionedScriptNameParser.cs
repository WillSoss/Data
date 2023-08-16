using System.Collections;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("WillSoss.Data.Tests")]

namespace WillSoss.Data
{
    internal class VersionedScriptNameParser : IEnumerable<(string version, string file)>
    {
        private static readonly Regex scriptPattern = new Regex(@"^(?<version>\d+(\.\d+){0,3})[-_\s]+(?<name>\w+)*\.sql$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        internal static (string version, string name) Parse(string file)
        {
            string? version = null;
            string? name = null;

            if (!TryParse(file, out version, out name))
                throw new InvalidScriptNameException(file, "Scripts must be named in the format '#[.#[.#[.#]]]-name.sql'");

            return (version!, name!);
        }

        internal static bool TryParse(string file, out string? version, out string? name)
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

        private readonly IEnumerable<string> _files;

        internal VersionedScriptNameParser(IEnumerable<string> files)
        {
            _files = files ?? throw new ArgumentNullException(nameof(files));
        }

        public IEnumerator<(string, string)> GetEnumerator()
        {
            foreach (var file in _files)
                yield return Parse(file);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
