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

using System.Linq;
using Cassette.Configuration;

namespace Cassette.Stylesheets
{
    class ExternalStylesheetHtmlRenderer : IBundleHtmlRenderer<ExternalStylesheetBundle>
    {
        readonly IBundleHtmlRenderer<StylesheetBundle> fallbackRenderer;
        readonly CassetteSettings settings;

        public ExternalStylesheetHtmlRenderer(IBundleHtmlRenderer<StylesheetBundle> fallbackRenderer, CassetteSettings settings)
        {
            this.fallbackRenderer = fallbackRenderer;
            this.settings = settings;
        }

        public string Render(ExternalStylesheetBundle bundle)
        {
            if (settings.IsDebuggingEnabled && bundle.Assets.Any())
            {
                return fallbackRenderer.Render(bundle);
            }
            else
            {
                if (string.IsNullOrEmpty(bundle.Media))
                {
                    return string.Format(HtmlConstants.LinkHtml, bundle.Url);
                }
                else
                {
                    return string.Format(HtmlConstants.LinkWithMediaHtml, bundle.Url, bundle.Media);
                }
            }
        }
    }
}
