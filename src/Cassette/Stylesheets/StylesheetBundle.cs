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

using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.Stylesheets
{
    public class StylesheetBundle : Bundle
    {
        public StylesheetBundle(string applicationRelativePath)
            : base(applicationRelativePath)
        {
            ContentType = "text/css";
            Processor = new StylesheetPipeline();
        }

        /// <summary>
        /// The value of the media attribute for this stylesheet's link element. For example, <example>print</example>.
        /// </summary>
        public string Media { get; set; }

        /// <summary>
        /// The Internet Explorer specific condition used control if the stylesheet should be loaded using an HTML conditional comment.
        /// For example, <example>"lt IE 9"</example>.
        /// </summary>
        public string Condition { get; set; }

        public IBundleProcessor<StylesheetBundle> Processor { get; set; }

        public IBundleHtmlRenderer<StylesheetBundle> Renderer { get; set; }


        internal override void Process(CassetteSettings settings)
        {
            Processor.Process(this, settings);
        }

        internal override string Render()
        {
            return Renderer.Render(this);
        }
    }
}