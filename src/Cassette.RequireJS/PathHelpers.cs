namespace Cassette.RequireJS
{
    public static class PathHelpers
    {
        public static string ConvertCassettePathToModulePath(string assetPath)
        {
            var path = assetPath.TrimStart('~', '/');
            return RemoveFileExtension(path);
        }

        static string RemoveFileExtension(string path)
        {
            var index = path.LastIndexOf('.');
            var slash = path.LastIndexOf('/');
            if (index >= 0 && index > slash)
            {
                path = path.Substring(0, index);
            }
            return path;
        }
    }
}