namespace WillSoss.Data.Cli
{
    internal interface ICliCommand
    {
        Task RunAsync(CancellationToken cancel);
    }
}
