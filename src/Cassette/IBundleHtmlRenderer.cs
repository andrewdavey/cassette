namespace Cassette
{
    /// <summary>
    /// Creates the HTML required to reference a bundle in a page.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="Bundle"/>.</typeparam>
    [ProtoBuf.ProtoContract]
    public interface IBundleHtmlRenderer<in T>
        where T : Bundle
    {
        /// <summary>
        /// Returns the HTML elements that will include the bundle in a page.
        /// </summary>
        /// <param name="bundle">The bundle to render.</param>
        /// <returns>The HTML elements that will include the bundle in a page.</returns>
        string Render(T bundle);
    }
}