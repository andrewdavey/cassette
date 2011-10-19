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

using System.IO;
using System.Web;
using Cassette.BundleProcessing;

namespace Cassette.Stylesheets
{
    public class StylesheetBundle : Bundle
    {
        public StylesheetBundle(string directory)
            : this(directory, null)
        {
        }

        internal StylesheetBundle(string path, BundleDescriptor bundleDescriptor)
            : base(path)
        {
            ContentType = "text/css";
            Processor = new StylesheetPipeline();
            BundleInitializers.Add(new BundleDirectoryInitializer(path)
            {
                FilePattern = "*.css;*.less",
                SearchOption = SearchOption.AllDirectories
            });
        }

        public string Media { get; set; }
        
        public IBundleProcessor<StylesheetBundle> Processor { get; set; }

        public IBundleHtmlRenderer<StylesheetBundle> Renderer { get; set; }

        public override void Process(ICassetteApplication application)
        {
            Processor.Process(this, application);
        }

        public override IHtmlString Render(ICassetteApplication application)
        {
            return Renderer.Render(this);
        }
    }
}
