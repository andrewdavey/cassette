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
using Cassette.ModuleProcessing;

namespace Cassette.Stylesheets
{
    public class StylesheetModule : Module
    {
        public StylesheetModule(string directory)
            : base(directory)
        {
            ContentType = "text/css";
            Processor = new StylesheetPipeline();
        }

        protected static readonly string LinkHtml = "<link href=\"{0}\" type=\"text/css\" rel=\"stylesheet\"/>";
        protected static readonly string LinkHtmlWithMedia = "<link href=\"{0}\" type=\"text/css\" rel=\"stylesheet\" media=\"{1}\"/>";

        public string Media { get; set; }
        
        public IModuleProcessor<StylesheetModule> Processor { get; set; }

        public IModuleHtmlRenderer<StylesheetModule> Renderer { get; set; }

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
