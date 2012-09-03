using System.Collections.Generic;

namespace Cassette.Spriting.Spritastic
{
    class SpritePackage
    {
        public SpritePackage(string generatedCss, IList<Sprite> sprites, IList<SpriteException> exceptions)
        {
            Exceptions = exceptions;
            Sprites = sprites;
            GeneratedCss = generatedCss;
        }

        public string GeneratedCss { get; private set; }
        public IList<Sprite> Sprites { get; private set; }
        public IList<SpriteException> Exceptions { get; private set; }
    }
}