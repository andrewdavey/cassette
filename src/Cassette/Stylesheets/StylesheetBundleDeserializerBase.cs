using Cassette.TinyIoC;

namespace Cassette.Stylesheets
{
    abstract class StylesheetBundleDeserializerBase<T> : BundleDeserializer<T>
        where T : StylesheetBundle
    {
        protected StylesheetBundleDeserializerBase(TinyIoCContainer container) 
            : base(container)
        {
        }

        protected void AssignStylesheetBundleProperties(T bundle)
        {
            bundle.Condition = GetOptionalAttribute("Condition");
            bundle.Renderer = CreateHtmlRenderer<StylesheetBundle>();
        }
    }
}