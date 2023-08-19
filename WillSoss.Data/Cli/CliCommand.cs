namespace WillSoss.Data.Cli
{
    internal abstract class CliCommand
    {
        internal Task RunAsync() => RunAsync(new CancellationTokenSource().Token);
        internal abstract Task RunAsync(CancellationToken cancel);
    }
}
