namespace dotless.Core.Input
{
    interface IFileReader
    {
        string GetFileContents(string fileName);

        bool DoesFileExist(string fileName);
    }
}