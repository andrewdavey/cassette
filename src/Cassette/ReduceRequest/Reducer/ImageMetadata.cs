namespace Cassette.ReduceRequest.Reducer
{
    public struct ImageMetadata
    {
        public ImageMetadata(BackgroundImageClass image)
            : this()
        {
            Url = image.ImageUrl;
            Width = image.Width ?? 0;
            Height = image.Height ?? 0;
            XOffset = image.XOffset.Offset;
            YOffset = image.YOffset.Offset;
        }

        public int Width { get; set; }
        public int Height { get; set; }
        public int XOffset { get; set; }
        public int YOffset { get; set; }
        public string Url { get; set; }
    }
}