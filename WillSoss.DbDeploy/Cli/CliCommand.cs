namespace WillSoss.DbDeploy.Cli
{
    internal interface ICliCommand
    {
        Task RunAsync(CancellationToken cancel);
    }
}
