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

using System.Text.RegularExpressions;
using Cassette;
using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;

namespace Example
{
    public class CassetteConfiguration : ICassetteConfiguration
    {
        public void Configure(ModuleConfiguration modules, ICassetteApplication application)
        {
            modules.Add(
                new PerSubDirectorySource<ScriptModule>("Scripts")
                {
                    FilePattern = "*.js",
                    Exclude = new Regex("-vsdoc\\.js$")
                },
                new ExternalScriptModule("twitter", "http://platform.twitter.com/widgets.js")
                {
                    Location = "body"
                }
            );

            modules.Add(new DirectorySource<StylesheetModule>("Styles")
            {
                FilePattern = "*.css;*.less",
                CustomizeModule = module => module.Processor = new StylesheetPipeline
                {
                    CompileLess = true,
                    ConvertImageUrlsToDataUris = true
                }
            });

            modules.Add(new PerSubDirectorySource<HtmlTemplateModule>("HtmlTemplates")
            {
                CustomizeModule = module => module.Processor = new KnockoutJQueryTmplPipeline()
            });
        }
    }
}
