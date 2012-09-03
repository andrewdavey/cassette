namespace Cassette.Spriting.Spritastic.ImageLoad
{
    public interface IImageLoader
    {
        string BasePath { get; set; }
        byte[] GetImageBytes(string url);
    }
}
