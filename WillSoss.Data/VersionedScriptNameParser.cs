using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("WillSoss.Data.Tests")]

namespace WillSoss.Data
{
    internal class VersionedScriptNameParser : IEnumerable<(string version, string file)>
    {
        private static readonly Regex scriptPattern = new Regex(@"^(?<version>\d+(\.\d+){0,3})[-_\s]+(?<name>\w+)*\.sql$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly IEnumerable<string> _files;

        public VersionedScriptNameParser(IEnumerable<string> files)
        {
            _files = files ?? throw new ArgumentNullException(nameof(files));
        }

        public IEnumerator<(string, string)> GetEnumerator()
        {
            foreach (var file in _files)
            {
                var match = scriptPattern.Match(Path.GetFileName(file));

                if (!match.Success)
                    throw new InvalidScriptNameException(file, "Scripts loaded from a directory must be named in the format '#[.#[.#[.#]]]-name.sql'");

                yield return (match.Groups["version"].Captures[0].Value, file);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
