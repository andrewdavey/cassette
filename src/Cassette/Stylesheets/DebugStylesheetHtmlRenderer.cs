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
    class DebugStylesheetHtmlRenderer : IBundleHtmlRenderer<StylesheetBundle>
    {
        public DebugStylesheetHtmlRenderer(IUrlGenerator urlGenerator)
        {
            this.urlGenerator = urlGenerator;
        }

        readonly IUrlGenerator urlGenerator;

        public string Render(StylesheetBundle bundle)
        {
            var assetUrls = GetAssetUrls(bundle);
            var createLink = GetCreateLinkFunc(bundle);

            return (
                string.Join(
                    Environment.NewLine,
                    assetUrls.Select(createLink)
                )
            );
        }

        IEnumerable<string> GetAssetUrls(StylesheetBundle bundle)
        {
            return bundle.Assets.Select(urlGenerator.CreateAssetUrl);
        }

        Func<string, string> GetCreateLinkFunc(StylesheetBundle bundle)
        {
            if (string.IsNullOrEmpty(bundle.Media))
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
                    bundle.Media
                );
            }
        }
    }
}
