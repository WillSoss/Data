
namespace WillSoss.DbDeploy
{
    public class InvalidFolderStructureException : Exception
    {
        public string Path { get; }

        public InvalidFolderStructureException(string path, string message)
            : base($"Invalid folder structure: {System.IO.Path.GetFileName(path)}. {message}")
        {
            Path = path;
        }
    }
}
