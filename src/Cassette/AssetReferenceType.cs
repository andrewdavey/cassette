namespace Cassette
{
    public enum AssetReferenceType
    {
        /// <summary>
        /// A reference to an asset in the same bundle as the referencing asset.
        /// </summary>
        SameBundle,
        /// <summary>
        /// A reference to an asset in another bundle, or to an entire other bundle itself.
        /// </summary>
        DifferentBundle,
        /// <summary>
        /// For example, a reference to an image from a CSS file.
        /// </summary>
        RawFilename,
        /// <summary>
        /// A direct reference to a URL.
        /// </summary>
        Url
    }
}
