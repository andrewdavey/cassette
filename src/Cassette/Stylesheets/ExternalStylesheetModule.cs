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
using Cassette.Utilities;

namespace Cassette.Stylesheets
{
    public class ExternalStylesheetModule : StylesheetModule, IModuleSource<StylesheetModule>, IExternalModule
    {
        public ExternalStylesheetModule(string url)
            : base(url)
        {
            this.url = url;
        }

        public ExternalStylesheetModule(string name, string url) 
            : base(PathUtilities.AppRelative(name))
        {
            this.url = url;
        }

        readonly string url;

        public override void Process(ICassetteApplication application)
        {
            // No processing required.
        }

        public override string Render(ICassetteApplication application)
        {
            if (string.IsNullOrEmpty(Media))
            {
                return String.Format(HtmlConstants.LinkHtml, url);
            }
            else
            {
                return String.Format(HtmlConstants.LinkWithMediaHtml, url, Media);
            }
        }

        public override bool ContainsPath(string path)
        {
            return base.ContainsPath(path) || url.Equals(path, StringComparison.OrdinalIgnoreCase);
        }

        IEnumerable<StylesheetModule> IModuleSource<StylesheetModule>.GetModules(IModuleFactory<StylesheetModule> moduleFactory, ICassetteApplication application)
        {
            yield return this;
        }
    }
}

