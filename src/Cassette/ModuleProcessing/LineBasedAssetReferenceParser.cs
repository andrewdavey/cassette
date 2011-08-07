using System.IO;
using System.Text.RegularExpressions;

namespace Cassette.ModuleProcessing
{
    public class LineBasedAssetReferenceParser<T> : ModuleProcessorOfAssetsMatchingFileExtension<T>
        where T : Module
    {
        public LineBasedAssetReferenceParser(string fileExtension, Regex referenceRegex)
            : base(fileExtension)
        {
            this.referenceRegex = referenceRegex;
        }

        readonly Regex referenceRegex;

        protected override void Process(IAsset asset)
        {
            using (var reader = new StreamReader(asset.OpenStream()))
            {
                int lineNumber = 0;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lineNumber++;
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    var match = referenceRegex.Match(line);
                    if (match.Success)
                    {
                        asset.AddReference(match.Groups[1].Value, lineNumber);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }
}
