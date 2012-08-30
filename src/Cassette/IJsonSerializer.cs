namespace Cassette
{
    public interface IJsonSerializer
    {
        string Serialize(object objectToSerialize);
    }
}