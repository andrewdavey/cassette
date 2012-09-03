namespace Cassette.Spriting.Spritastic
{
    interface ISpriteGenerator
    {
        SpritePackage GenerateFromCss(string cssContent, string cssPath);
    }
}