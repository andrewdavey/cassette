namespace Cassette
{
    public interface IBootstrapper
    {
        void Initialize();
        ICassetteApplication GetApplication();
    }
}