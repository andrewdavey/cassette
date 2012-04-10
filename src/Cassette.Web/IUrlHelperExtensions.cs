#if !NET35
namespace Cassette.Web
{
    public interface IUrlHelperExtensions
    {
        string CassetteFile(string applicationRelativeFilePath);
    }
}
#endif