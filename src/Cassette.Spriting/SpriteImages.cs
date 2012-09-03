using System;
using Cassette.BundleProcessing;
using Cassette.IO;
using Cassette.Stylesheets;

namespace Cassette.Spriting
{
    public class SpriteImages : IBundleProcessor<StylesheetBundle>
    {
        public SpriteImages(IDirectory cacheDirectory)
        {
            
        }

        public void Process(StylesheetBundle bundle)
        {
            throw new NotImplementedException();
        }
    }
}