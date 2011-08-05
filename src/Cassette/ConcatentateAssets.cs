using System.IO;

namespace Cassette
{
    public class ConcatentateAssets : IModuleProcessor<Module>
    {
        public void Process(Module module)
        {
            var outputStream = CopyAssetsIntoSingleStream(module);
            
            module.Assets.Clear();
            module.Assets.Add(new InMemoryAsset(outputStream));
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
