using System.Linq;
using System.IO;
using System.Text;
using System.Web;
using System.Collections.Generic;

namespace Cassette.Web
{
    public class HtmlTemplateAssetManager
    {
        public HtmlTemplateAssetManager(IReferenceBuilder referenceBuilder)
        {
            this.referenceBuilder = referenceBuilder;
        }

        readonly IReferenceBuilder referenceBuilder;

        public void Reference(params string[] paths)
        {
            foreach (var path in paths)
            {
                referenceBuilder.AddReference(path);
            }
        }

        public IHtmlString Render(string location = "")
        {
            return new HtmlString(CreateHtmlTemplatesHtml(location));
        }

        string CreateHtmlTemplatesHtml(string location)
        {
            var builder = new StringBuilder();
            foreach (var module in GetModules(location))
            {
                using (var stream = referenceBuilder.ModuleContainer.OpenModuleFile(module))
                using (var reader = new StreamReader(stream))
                {
                    builder.AppendLine(reader.ReadToEnd());
                }
            }
            return builder.ToString();
        }

        IEnumerable<Module> GetModules(string location)
        {
            return referenceBuilder.GetRequiredModules().Where(
                module => module.Location == location
            );
        }
    }
}
