using System;
using System.Collections.Generic;
using System.IO;
using Cassette.Configuration;
using Cassette.ReduceRequest.Reducer;
using Cassette.Utilities;
using CssImageTransformer = Cassette.ReduceRequest.Reducer.CssImageTransformer;
using ICssImageTransformer = Cassette.ReduceRequest.Reducer.ICssImageTransformer;
using SpriteManager = Cassette.ReduceRequest.Reducer.SpriteManager;
using SpritedImage = Cassette.ReduceRequest.Reducer.SpritedImage;

namespace Cassette
{
    internal class ImageSpriteTransformer : IAssetTransformer
    {
        readonly ICssImageTransformer cssImageTransformer = new CssImageTransformer(new CssSelectorAnalyzer());

        readonly CassetteSettings settings;
        readonly SpriteManager spriteManager;

        public ImageSpriteTransformer(CassetteSettings settings)
        {
            this.settings = settings;
            spriteManager = new SpriteManager(this.settings);
        }
        
        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return delegate
            {
                var css = ReadCss(openSourceStream);
                var extractImages = cssImageTransformer.ExtractImageUrls(css);

                //SpriteManager.SpritedCssKey = new Guid(new MD5CryptoServiceProvider().ComputeHash(asset.Hash));
                foreach (var image in extractImages)
                {
                    spriteManager.Add(image);

                    //TODO: Get sprite url and add to asset reference
                    //asset.AddRawFileReference(spriteUrl);
                }

                spriteManager.Dispose();

                var newCss = css;
                foreach (SpritedImage image in spriteManager)
                {
                    ((List<AssetReference>)asset.References).Add(new AssetReference("~" + image.Url, asset, -1, AssetReferenceType.RawFilename));
                    newCss = cssImageTransformer.InjectSprite(newCss, image);
                }

                return newCss.AsStream();
            };
        }

        string ReadCss(Func<Stream> openSourceStream)
        {
            using (var reader = new StreamReader(openSourceStream()))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
