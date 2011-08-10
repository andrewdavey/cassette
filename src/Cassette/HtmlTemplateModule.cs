using System;
using System.IO;
using System.Web;

namespace Cassette
{
    public class HtmlTemplateModule : Module
    {
        public HtmlTemplateModule(string directory, IFileSystem fileSystem)
            : base(directory, fileSystem)
        {
            ContentType = "text/html";
        }

        public override IHtmlString Render(ICassetteApplication application)
        {
            if (application.IsOutputOptimized)
            {
                // Already rendered the templates into a single block.
                // So just return it.
                using (var reader = new StreamReader(Assets[0].OpenStream()))
                {
                    return new HtmlString(reader.ReadToEnd());
                }
            }
            else
            {
                return new HtmlString(RenderAllAssets());
            }
        }

        public string RenderAllAssets()
        {
            var writer = new StringWriter();
            var first = true;
            foreach (var asset in Assets)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    writer.WriteLine();
                }
                using (var reader = new StreamReader(asset.OpenStream()))
                {
                    writer.Write(reader.ReadToEnd());
                }
            }
            return writer.ToString();
        }

        public string RenderTemplate(Func<Stream> openStream, IAsset asset)
        {
            return string.Format(
                "<script id=\"{0}\" type=\"{1}\">{3}{2}{3}</script>",
                TemplateId(asset.SourceFilename),
                ContentType,
                TemplateBody(openStream),
                Environment.NewLine
            );
        }

        string TemplateId(string filename)
        {
            var id = filename.Replace('\\', '-').Replace('/', '-');
            var index = id.LastIndexOf('.');
            if (index > 0)
            {
                return id.Substring(0, index);
            }
            return id;
        }

        string TemplateBody(Func<Stream> openStream)
        {
            using (var reader = new StreamReader(openStream()))
            {
                return reader.ReadToEnd();
            }
        }
    }
}