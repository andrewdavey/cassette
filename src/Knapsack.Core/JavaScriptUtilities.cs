namespace Knapsack
{
    public static class JavaScriptUtilities
    {
        public static string EscapeJavaScriptString(string s)
        {
            return s.Replace(@"\", @"\\")
                    .Replace("'", @"\'")
                    .Replace("\"", @"\""")
                    .Replace("\r", @"\r")
                    .Replace("\n", @"\n");
        }
    }
}