namespace Cassette.Aspnet
{
    interface ICassetteRequestHandler
    {
        void ProcessRequest(string path);
    }
}