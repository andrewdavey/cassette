namespace Cassette
{
    public interface IContainerBuilder
    {
        void Build(TinyIoC.TinyIoCContainer container);
    }
}