namespace WillSoss.Data
{
    internal static class ExtensionMethods
    {
        internal static Dictionary<string,string> ToLowercaseKeys(this Dictionary<string,string> dictionary)
        {
            var d = new Dictionary<string,string>();
            
            foreach (var kv in dictionary)
                d.Add(kv.Key.ToLowerInvariant(), kv.Value);
            
            return d;
        }

        /// <summary>
        /// Fills Build and Revision with 0 when not specified so that, for example, 1.1 is equal to 1.1.0.0.
        /// </summary>
        internal static Version FillZeros(this Version version) => new Version(version.Major, version.Minor, Math.Max(version.Build, 0), Math.Max(version.Revision, 0));
    }
}
