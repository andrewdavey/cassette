#region License
/*
Copyright 2011 Andrew Davey

This file is part of Cassette.

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
*/
#endregion

using System.Web;
using Cassette.BundleProcessing;
using System.IO;

namespace Cassette.HtmlTemplates
{
    public class HtmlTemplateBundle : Bundle
    {
        public HtmlTemplateBundle(string path)
            : this(path, null)
        {
        }

        internal HtmlTemplateBundle(string path, BundleDescriptor bundleDescriptor)
            : base(path)
        {
            ContentType = "text/html";
            Processor = new HtmlTemplatePipeline();
            BundleInitializers.Add(new BundleDirectoryInitializer(path)
            {
                FilePattern = "*.htm;*.html",
                SearchOption = SearchOption.AllDirectories,
                BundleDescriptor = bundleDescriptor
            });
        }

        public IBundleProcessor<HtmlTemplateBundle> Processor { get; set; }
        
        public IBundleHtmlRenderer<HtmlTemplateBundle> Renderer { get; set; }

        public override void Process(ICassetteApplication application)
        {
            Processor.Process(this, application);
        }

        public override IHtmlString Render()
        {
            return Renderer.Render(this);
        }

        public string GetTemplateId(IAsset asset)
        {
            var path = asset.SourceFile.FullPath
                .Substring(Path.Length + 1)
                .Replace(System.IO.Path.DirectorySeparatorChar, '-')
                .Replace(System.IO.Path.AltDirectorySeparatorChar, '-');
            return System.IO.Path.GetFileNameWithoutExtension(path);
        }
    }
}
