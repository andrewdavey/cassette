using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace Cassette
{
    public class ConcatentateAssets : IModuleProcessor<Module>
    {
        public void Process(Module module)
        {
            var filenames = GetAssetFilenames(module);
            var outputStream = CopyAssetsIntoSingleStream(module);
            var references = MergeReferences(module);
            module.Assets.Clear();
            module.Assets.Add(new InMemoryAsset(filenames, outputStream, references));
        }

        string[] GetAssetFilenames(Module module)
        {
            return module.Assets.Select(a => a.SourceFilename).ToArray();
        }

        MemoryStream CopyAssetsIntoSingleStream(Module module)
        {
            var outputStream = new MemoryStream();
            var writer = new StreamWriter(outputStream);
            var isFirstAsset = true;
            foreach (var asset in module.Assets)
            {
                if (isFirstAsset)
                {
                    isFirstAsset = false;
                }
                else
                {
                    writer.WriteLine();
                }
                WriteAsset(asset, writer);
            }

            writer.Flush();
            outputStream.Position = 0;
            return outputStream;
        }

        AssetReference[] MergeReferences(Module module)
        {
            var all = from asset in module.Assets
                      from reference in asset.References
                      select reference;
            return all.ToArray();
        }

        void WriteAsset(IAsset asset, StreamWriter writer)
        {
            using (var reader = new StreamReader(asset.OpenStream()))
            {
                var isFirstLine = true;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (isFirstLine)
                    {
                        isFirstLine = false;
                    }
                    else
                    {
                        writer.WriteLine();
                    }
                    writer.Write(line);
                }
            }
        }
    }
}
