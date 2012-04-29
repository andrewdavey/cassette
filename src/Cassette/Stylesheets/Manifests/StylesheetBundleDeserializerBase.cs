using Cassette.IO;
using Cassette.Manifests;

namespace Cassette.Stylesheets.Manifests
{
    abstract class StylesheetBundleDeserializerBase<T> : BundleDeserializer<T>
        where T : StylesheetBundle
    {
        protected StylesheetBundleDeserializerBase(IDirectory directory, IUrlModifier urlModifier) 
            : base(directory, urlModifier)
        {
        }

        protected void AssignStylesheetBundleProperties(T bundle)
        {
            bundle.Condition = GetOptionalAttribute("Condition");
            bundle.Media = GetOptionalAttribute("Media");
            bundle.Renderer = CreateHtmlRenderer<StylesheetBundle>();
        }
    }
}