using System.IO;
using Cassette.IO;

namespace Cassette
{
    class AddFileBundleInitializer : IBundleInitializer
    {
        readonly string filePath;

        public AddFileBundleInitializer(string filePath)
        {
            this.filePath = filePath;
        }

        public void InitializeBundle(Bundle bundle, ICassetteApplication application)
        {
            Trace.Source.TraceInformation("Adding file \"{0}\" to bundle \"{1}\".", filePath, bundle.Path);

            var file = GetFile(bundle, application);
            if (!file.Exists)
            {
                throw new FileNotFoundException(string.Format("File not found \"{0}\" for bundle \"{1}\".", filePath, bundle.Path));
            }

            bundle.Assets.Add(new Asset(bundle, file));
        }

        IFile GetFile(Bundle bundle, ICassetteApplication application)
        {
            if (filePath.StartsWith("~"))
            {
                return application.SourceDirectory.GetFile(filePath);
            }
            else
            {
                return application.SourceDirectory
                    .GetDirectory(bundle.Path)
                    .GetFile(filePath);
            }
        }
    }
}