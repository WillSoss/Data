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
    }
}
