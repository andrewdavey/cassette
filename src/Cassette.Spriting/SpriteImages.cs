using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Cassette.BundleProcessing;
using Cassette.IO;
using Cassette.Spriting.Spritastic;
using Cassette.Stylesheets;

namespace Cassette.Spriting
{
    public class SpriteImages : IBundleProcessor<StylesheetBundle>
    {
        readonly CassetteSettings settings;
        readonly Func<ISpriteGenerator> createSpriteGenerator;

        internal SpriteImages(CassetteSettings settings, Func<ISpriteGenerator> createSpriteGenerator)
        {
            this.settings = settings;
            this.createSpriteGenerator = createSpriteGenerator;
        }

        public void Process(StylesheetBundle bundle)
        {
            if (settings.IsDebuggingEnabled) return;
            if (bundle.Assets.Count == 0) return;

            var css = ReadCssFromBundle(bundle);
            var spritePackage = GenerateSprites(bundle, css);
            ReplaceBundleCss(bundle, spritePackage.GeneratedCss);
            SaveSpritesToCache(spritePackage.Sprites);
        }

        void SaveSpritesToCache(IEnumerable<Sprite> sprites)
        {
            var spritesDirectory = GetOrCreateSpritesDirectory();
            foreach (var sprite in sprites)
            {
                SaveSpriteToCache(spritesDirectory, sprite);
            }
        }

        IDirectory GetOrCreateSpritesDirectory()
        {
            var spritesDirectory = settings.CacheDirectory.GetDirectory("sprites");
            spritesDirectory.Create();
            return spritesDirectory;
        }

        void SaveSpriteToCache(IDirectory spritesDirectory, Sprite sprite)
        {
            var path = Regex.Replace(sprite.Url, @"/cassette\.axd/cached/sprites/(.*)$", "$1");
            var file = spritesDirectory.GetFile(path);
            using (var fileStream = file.Open(FileMode.Create, FileAccess.Write, FileShare.None))
            {
                fileStream.Write(sprite.Image, 0, sprite.Image.Length);
                fileStream.Flush();
            }
        }

        string ReadCssFromBundle(StylesheetBundle bundle)
        {
            return bundle.Assets[0].GetTransformedContent();
        }

        SpritePackage GenerateSprites(StylesheetBundle bundle, string css)
        {
            var generator = createSpriteGenerator();
            return generator.GenerateFromCss(css, bundle.Path);
        }

        void ReplaceBundleCss(StylesheetBundle bundle, string newCss)
        {
            var spritedCss = new SpritedCss(newCss, bundle.Assets[0]);
            bundle.Assets.Clear();
            bundle.Assets.Add(spritedCss);

            bundle.Hash = spritedCss.Hash;
        }
    }
}