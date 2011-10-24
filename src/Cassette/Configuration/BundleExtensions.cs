namespace Cassette.Configuration
{
    public static class BundleExtensions
    {
        /// <summary>
        /// Adds a file asset to the bundle.
        /// </summary>
        /// <param name="bundle">The bundle to add the file asset to.</param>
        /// <param name="filePath">The path to the file. Either relative to the bundle path, or application relative by starting with "~/".</param>
        public static void AddFile(this Bundle bundle, string filePath)
        {
            bundle.BundleInitializers.Add(new AddFileBundleInitializer(filePath));
        }
    }
}