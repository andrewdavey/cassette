using System;
using System.IO;
using Cassette.ModuleBuilding;

namespace Cassette.Assets.Stylesheets
{
    public class UnresolvedStylesheetModuleBuilder : UnresolvedModuleBuilder
    {
        readonly string applicationRoot;

        public UnresolvedStylesheetModuleBuilder(string rootDirectory, string applicationRoot)
            : base(rootDirectory, new[] { "css", "less" })
        {
            this.applicationRoot = applicationRoot;
        }

        protected override IUnresolvedAssetParser CreateParser(string filename)
        {
            var extension = Path.GetExtension(filename).ToLowerInvariant();
            switch (extension){
                case ".css":
                    return new UnresolvedCssParser(applicationRoot);

                case ".less":
                    return new UnresolvedLessParser();

                default:
                    throw new ArgumentException("Unknown stylesheet file extension: " + extension);
            }
        }
    }
}
