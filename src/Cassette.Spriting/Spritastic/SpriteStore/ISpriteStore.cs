namespace Cassette.Spriting.Spritastic.SpriteStore
{
    interface ISpriteStore
    {
        string SaveSpriteAndReturnUrl(byte[] spriteBytes);
    }
}
