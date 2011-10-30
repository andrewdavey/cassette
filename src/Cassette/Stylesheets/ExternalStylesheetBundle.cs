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

namespace Cassette.Stylesheets
{
    class ExternalStylesheetBundle : StylesheetBundle
    {
        public ExternalStylesheetBundle(string url)
            : base(url)
        {
            this.url = url;
        }

        public ExternalStylesheetBundle(string url, string applicationRelativePath) 
            : base(applicationRelativePath)
        {
            this.url = url;
        }

        readonly string url;
        ExternalStylesheetHtmlRenderer externalHtmlRender;

        internal string Url
        {
            get { return url; }
        }

        internal override void Process(ICassetteApplication application)
        {
            base.Process(application);
            externalHtmlRender = new ExternalStylesheetHtmlRenderer(Renderer, application);
        }

        internal override IHtmlString Render()
        {
            return externalHtmlRender.Render(this);
        }

        internal override bool ContainsPath(string pathToFind)
        {
            return base.ContainsPath(pathToFind) || url.Equals(pathToFind, StringComparison.OrdinalIgnoreCase);
        }
    }
}