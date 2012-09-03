using System;
using System.Drawing;
using Cassette.Spriting.Spritastic.Parser;

namespace Cassette.Spriting.Spritastic.Generator
{
    class SpritedImage
    {
        public SpritedImage(int averageColor, BackgroundImageClass cssClass, Bitmap image)
        {
            AverageColor = averageColor;
            CssClass = cssClass;
            Image = image;
            Position = -1;
        }

        public Bitmap Image { get; set; }
        public int AverageColor { get; set; }
        public int Position { get; set; }
        public string Url { get; set; }
        public SpriteManager.ImageMetadata Metadata { get; set; }
        public BackgroundImageClass CssClass { get; set; }

        public string Render()
        {
            var newClass = CssClass.ImageUrl != null && CssClass.OriginalClassString.IndexOf(CssClass.ImageUrl, StringComparison.OrdinalIgnoreCase) > -1
                                  ? CssClass.OriginalClassString.ToLower().Replace(CssClass.ImageUrl.ToLower(), Url)
                                  : CssClass.OriginalClassString.ToLower().Replace("}",
                                                                                   string.Format(
                                                                                       ";background-image: url('{0}')}}", Url));
            newClass = newClass.Replace(CssClass.Selector.ToLower(), CssClass.Selector);
            var yOffset = "0";
            if (CssClass.YOffset.PositionMode == PositionMode.Unit && CssClass.YOffset.Offset > 0)
                yOffset = string.Format("{0}px", CssClass.YOffset.Offset);
            return newClass.Replace("}",
                                        string.Format(";background-position: -{0}px {1}{2};}}",
                                        Position, yOffset, CssClass.Important ? " !important" : string.Empty));
        }

        public virtual string InjectIntoCss(string originalCss)
        {
            return originalCss.Replace(CssClass.OriginalClassString, Render());
        }
    }
}