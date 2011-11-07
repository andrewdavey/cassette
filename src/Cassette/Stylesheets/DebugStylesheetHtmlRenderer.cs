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

using System;
using System.Collections.Generic;
using System.Linq;

namespace Cassette.Stylesheets
{
    public class DebugStylesheetHtmlRenderer : IModuleHtmlRenderer<StylesheetModule>
    {
        public DebugStylesheetHtmlRenderer(IUrlGenerator urlGenerator)
        {
            this.urlGenerator = urlGenerator;
        }

        readonly IUrlGenerator urlGenerator;

        public string Render(StylesheetModule module)
        {
            var assetUrls = GetAssetUrls(module);
            var createLink = GetCreateLinkFunc(module);

            return string.Join(
                    Environment.NewLine,
                    assetUrls.Select(createLink)
                );
        }

        IEnumerable<string> GetAssetUrls(StylesheetModule module)
        {
            return module.Assets.Select(
                asset => asset.HasTransformers 
                    ? urlGenerator.CreateAssetCompileUrl(module, asset)
                    : urlGenerator.CreateAssetUrl(asset)
            );
        }

        Func<string, string> GetCreateLinkFunc(StylesheetModule module)
        {
            if (string.IsNullOrEmpty(module.Media))
            {
                return url => string.Format(
                    HtmlConstants.LinkHtml,
                    url
                );
            }
            else
            {
                return url => string.Format(
                    HtmlConstants.LinkWithMediaHtml,
                    url,
                    module.Media
                );
            }
        }
    }
}
