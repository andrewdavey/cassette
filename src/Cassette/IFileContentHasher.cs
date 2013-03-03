namespace Cassette
{
    public interface IFileContentHasher
    {
        byte[] Hash(string filename);
    }
}