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
using System.Web;
using Cassette.Utilities;

namespace Cassette.Stylesheets
{
    public class ExternalStylesheetBundle : StylesheetBundle
    {
        public ExternalStylesheetBundle(string url)
            : base(url)
        {
            this.url = url;
        }

        public ExternalStylesheetBundle(string name, string url) 
            : base(PathUtilities.AppRelative(name))
        {
            this.url = url;
        }

        internal ExternalStylesheetBundle(string path, BundleDescriptor bundleDescriptor)
            : base(path, bundleDescriptor)
        {
            url = path.IsUrl() ? path : bundleDescriptor.ExternalUrl;
        }

        readonly string url;

        public override void Process(ICassetteApplication application)
        {
            // No processing required.
        }

        public override IHtmlString Render(ICassetteApplication application)
        {
            if (string.IsNullOrEmpty(Media))
            {
                return new HtmlString(String.Format(HtmlConstants.LinkHtml, url));
            }
            else
            {
                return new HtmlString(String.Format(HtmlConstants.LinkWithMediaHtml, url, Media));
            }
        }

        public override bool ContainsPath(string path)
        {
            return base.ContainsPath(path) || url.Equals(path, StringComparison.OrdinalIgnoreCase);
        }
    }
}