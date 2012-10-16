namespace Cassette
{
    public class SimpleJsonSerializer : IJsonSerializer
    {
        public string Serialize(object objectToSerialize)
        {
            return SimpleJson.SerializeObject(objectToSerialize);
        }
    }
}