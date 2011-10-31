namespace Cassette.Configuration
{
    public enum DebugMode
    {
        /// <summary>
        /// Inherit debug mode setting from the current application. For example, &lt;system.web&gt;/&lt;compilation debug="true"&gt;
        /// </summary>
        Inherit,
        On,
        Off
    }
}