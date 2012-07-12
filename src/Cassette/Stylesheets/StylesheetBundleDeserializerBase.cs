namespace Cassette.Stylesheets
{
    abstract class StylesheetBundleDeserializerBase<T> : BundleDeserializer<T>
        where T : StylesheetBundle
    {
        protected StylesheetBundleDeserializerBase(IUrlModifier urlModifier, IApplicationRootPrepender applicationRootPrepender)
            : base(urlModifier, applicationRootPrepender)
        {
        }

        protected void AssignStylesheetBundleProperties(T bundle)
        {
            bundle.Condition = GetOptionalAttribute("Condition");
            bundle.Renderer = CreateHtmlRenderer<StylesheetBundle>();
        }
    }
}