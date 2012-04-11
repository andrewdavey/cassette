namespace Cassette
{
    /// <summary>
    /// A task to run once at application start up.
    /// </summary>
    public interface IStartUpTask
    {
        void Start();
    }
}