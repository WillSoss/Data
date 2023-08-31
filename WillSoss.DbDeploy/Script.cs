using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace WillSoss.DbDeploy
{
    public partial class Script
    {
        static readonly Regex _go = GetGoRegex();
        private readonly string[] _batches;

        public string Name { get; private set; } = string.Empty;
        public Version Version { get; private set; } = new Version().FillZeros();
        public bool IsVersioned => Version != new Version(0, 0);
        public string Location { get; }
        public string FileName { get; }
        public string Body { get; }
        public IEnumerable<string> Batches { get { return _batches; } }

        public Script(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) 
                throw new ArgumentNullException(nameof(path));

            if (!File.Exists(path))
                throw new FileNotFoundException("File not found.", path);

            SetVersionFromFileName(Path.GetFileName(path));

            using var stream = File.OpenRead(path);

            Body = ReadStream(stream);
            _batches = GetBatches(Body);

            Location = Path.GetDirectoryName(path)!;
            FileName = Path.GetFileName(path);
        }

        public Script(Assembly assembly, string @namespace, string filename)
        { 
            string resource = $"{@namespace}.{@filename}";

            using var stream = assembly!.GetManifestResourceStream(resource);

            if (stream == null)
            {
                string found = assembly.GetManifestResourceNames().Aggregate(new StringBuilder(), (sb, r) => sb.AppendLine(r)).ToString();
                throw new ArgumentException($"Could not find embedded resource '{resource}'. Embedded resources found in in assembly '{assembly.FullName}': {found}");
            }

            SetVersionFromFileName(filename);

            Body = ReadStream(stream);
            _batches = GetBatches(Body);

            Location = resource;
            FileName = filename;
        }

        void SetVersionFromFileName(string filename)
        {
            if (Parser.TryParseFileName(filename, out string? version, out string? name))
            {
                // Version class requires at least major.minor
                Version = Version.Parse(version!.IndexOf('.') < 0 ? $"{version}.0" : version).FillZeros();
                Name = name!;
            }
            else
            {
                Version = new Version(0, 0, 0, 0);
                Name = Path.GetFileNameWithoutExtension(filename);
            }
        }

        string ReadStream(Stream stream)
        {
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        string[] GetBatches(string script) => _go.Split(script).Where(c => !_go.IsMatch(c) && !string.IsNullOrWhiteSpace(c)).ToArray();

        [GeneratedRegex("^\\s*go\\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled, "en-US")]
        private static partial Regex GetGoRegex();
    }
}
