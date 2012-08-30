#if NET35
namespace Cassette.Views
{
    public interface IHtmlString
    {
        string ToHtmlString();
    }

    public class HtmlString : IHtmlString
    {
        string _htmlString;

        public HtmlString(string htmlString)
        {
            this._htmlString = htmlString;
        }

        public string ToHtmlString()
        {
            return _htmlString;
        }

        public override string ToString()
        {
            return this._htmlString;
        }
    }
}
#endif