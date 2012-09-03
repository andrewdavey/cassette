namespace Cassette.Spriting.Spritastic
{
    class Sprite
    {
        public Sprite(string url, byte[] image)
        {
            Image = image;
            Url = url;
        }

        public string Url { get; private set; }
        public byte[] Image { get; private set; }
    }
}