using System.CommandLine.Parsing;

namespace WillSoss.Data.Cli
{
    internal class CliInvoker
    {
        private readonly string[] _args;
        private readonly Parser _parser;

        internal CliInvoker(string[] args, Parser parser)
        {
            _args = args;
            _parser = parser;
        }

        internal async Task Invoke() => await _parser.InvokeAsync(_args);
    }
}
