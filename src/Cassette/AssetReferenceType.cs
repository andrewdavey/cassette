namespace Cassette
{
    public enum AssetReferenceType
    {
        /// <summary>
        /// A reference to an asset in the same module as the referencing asset.
        /// </summary>
        SameModule,
        /// <summary>
        /// A reference to an asset in another module, or to an entire other module itself.
        /// </summary>
        DifferentModule,
        /// <summary>
        /// For example, a reference to an image from a CSS file.
        /// </summary>
        RawFilename
    }
}