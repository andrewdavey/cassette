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
using System.Web;

namespace Cassette.Views
{
    // Partial backwards compatibility with Cassette 0.8.
    // Allow people to keep using the Assets helper in their views.

    [Obsolete("The Assets helper class is obsolete. Use the Bundles helper class instead.")]
    public static class Assets
    {
        public static ObsoleteBundleHelper Scripts
        {
            get { return new ObsoleteScriptHelper(); }
        }

        public static ObsoleteBundleHelper Stylesheets
        {
            get { return new ObsoleteBundleHelper(Bundles.RenderStylesheets); }
        }

        public static ObsoleteBundleHelper HtmlTemplates
        {
            get { return new ObsoleteBundleHelper(Bundles.RenderHtmlTemplates); }
        }
    }

    [Obsolete("This class will be removed from the next version of Cassette. Do not use.")]
    public class ObsoleteBundleHelper
    {
        readonly Func<string, IHtmlString> render;

        internal ObsoleteBundleHelper(Func<string, IHtmlString> render)
        {
            this.render = render;
        }

        public void Reference(string path, string pageLocation = null)
        {
            Bundles.Reference(path, pageLocation);
        }

        public IHtmlString Render(string pageLocation = null)
        {
            return render(pageLocation);
        }

        public string ModuleUrl(string path)
        {
            return Bundles.Url(path);
        }
    }

    [Obsolete]
    public class ObsoleteScriptHelper : ObsoleteBundleHelper
    {
        public ObsoleteScriptHelper()
            : base(Bundles.RenderScripts)
        {
        }

        public void AddInline(string scriptContent, string pageLocation = null)
        {
            Bundles.AddInlineScript(scriptContent, pageLocation);
        }

        public void AddPageData(string globalVariable, object data, string pageLocation = null)
        {
            Bundles.AddPageData(globalVariable, data, pageLocation);
        }

// ReSharper disable ParameterTypeCanBeEnumerable.Global
        public void AddPageData(string globalVariable, IDictionary<string, object> data, string pageLocation = null)
// ReSharper restore ParameterTypeCanBeEnumerable.Global
        {
            Bundles.AddPageData(globalVariable, data, pageLocation);
        }
    }
}
