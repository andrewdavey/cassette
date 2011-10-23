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

namespace Cassette.Stylesheets
{
    public class StylesheetBundle : Bundle
    {
        public StylesheetBundle(string applicationRelativePath, bool useDefaultBundleInitializer)
            : base(applicationRelativePath, useDefaultBundleInitializer)
        {
            ContentType = "text/css";
            Processor = new StylesheetPipeline();
        }

        public StylesheetBundle(string applicationRelativePath)
            : this(applicationRelativePath, true)
        {
        }

        /// <summary>
        /// The value of the media attribute for this stylesheet's link element. For example, <example>print</example>.
        /// </summary>
        public string Media { get; set; }
        
        public IBundleProcessor<StylesheetBundle> Processor { get; set; }

        public IBundleHtmlRenderer<StylesheetBundle> Renderer { get; set; }

        public override void Process(ICassetteApplication application)
        {
            Processor.Process(this, application);
        }

        public override IHtmlString Render()
        {
            return Renderer.Render(this);
        }
    }
}
