using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cassette.ReduceRequest.Reducer;
using Cassette.Utilities;

namespace Cassette.Stylesheets
{
    internal class SpriteReferenceTransformer : IAssetTransformer
    {
        readonly IEnumerable<SpritedImage> spritedImages;

        public SpriteReferenceTransformer(IEnumerable<SpritedImage> spritedImages)
        {
            this.spritedImages = spritedImages;
        }

        #region IAssetTransformer Members

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return delegate
            {
                string css = ReadCss(openSourceStream);

                // Transform the references in the CSS with the new sprite urls
                foreach (SpritedImage image in spritedImages)
                {
                    var replace = image.CssClass.OriginalClassString;
                    var newstring = image.Render();

                    css = css.Replace(replace, newstring);
                }

                return css.AsStream();
            };
        }

        #endregion

        static string ReadCss(Func<Stream> openSourceStream)
        {
            using (var reader = new StreamReader(openSourceStream()))
            {
                return reader.ReadToEnd();
            }
        }
    }
}