namespace Knapsack
{
    public static class JavaScriptUtilities
    {
        public static string EscapeJavaScriptString(string sourceCode)
        {
            return sourceCode
                    .Replace(@"\", @"\\")
                    .Replace("'", @"\'")
                    .Replace("\"", @"\""")
                    .Replace("\r", @"\r")
                    .Replace("\n", @"\n");
        }

    }
}