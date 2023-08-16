namespace WillSoss.Data
{
    public class InvalidScriptNameException : Exception
    {
        public string Path { get; }

        public InvalidScriptNameException(string path, string message)
            : base($"Invalid script name: {System.IO.Path.GetFileName(path)}. {message}")
        {
            Path = path;
        }
    }
}
