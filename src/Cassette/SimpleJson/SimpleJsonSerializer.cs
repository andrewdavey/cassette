namespace Cassette
{
    class SimpleJsonSerializer : IJsonSerializer
    {
        public string Serialize(object objectToSerialize)
        {
            return SimpleJson.SerializeObject(objectToSerialize);
        }
    }
}