namespace WillSoss.DbDeploy
{
    public class InvalidFolderNameException : Exception
    {
        public string Path { get; }

        public InvalidFolderNameException(string path, string message)
            : base($"Invalid folder name: {System.IO.Path.GetFileName(path)}. {message}")
        {
            Path = path;
        }
    }
}
