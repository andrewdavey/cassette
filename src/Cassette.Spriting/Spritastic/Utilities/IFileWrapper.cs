namespace Cassette.Spriting.Spritastic.Utilities
{
    interface IFileWrapper
    {
        void Save(string content, string fileName);
        void Save(byte[] content, string fileName);
        bool FileExists(string path);
        byte[] GetFileBytes(string path);
    }
}