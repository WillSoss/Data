using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace WillSoss.Data
{
    public class Script
    {
        static readonly Regex _go = new Regex(@"^\s*go\s*$", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);
        private readonly string[] _batches;

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

            Body = ReadStream(stream);
            _batches = GetBatches(Body);

            Location = resource;
            FileName = filename;
        }

        string ReadStream(Stream stream)
        {
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        string[] GetBatches(string script) => _go.Split(script).Where(c => !_go.IsMatch(c) && !string.IsNullOrWhiteSpace(c)).ToArray();
    }
}
