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

namespace Cassette.Scripts
{
    public class ExternalScriptBundle : ScriptBundle
    {
        readonly string url;
        readonly string fallbackCondition;
        ExternalScriptBundleHtmlRenderer externalRenderer;

        public ExternalScriptBundle(string url)
            : base(url)
        {
            ValidateUrl(url);
            this.url = url;
        }

        public ExternalScriptBundle(string url, string bundlePath)
            : base(bundlePath)
        {
            ValidateUrl(url);
            this.url = url;
        }

        public ExternalScriptBundle(string url, string bundlePath, string fallbackCondition)
            : base(bundlePath)
        {
            ValidateUrl(url);
            this.url = url;
            this.fallbackCondition = fallbackCondition;
        }

        static void ValidateUrl(string url)
        {
            if (url == null) throw new ArgumentNullException("url");
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("URL is required.", "url");
            if (!url.IsUrl()) throw new ArgumentException(string.Format("Invalid URL: {0}", url), "url");
        }

        internal string Url
        {
            get { return url; }
        }

        internal string FallbackCondition
        {
            get { return fallbackCondition; }
        }

        public override void Initialize(ICassetteApplication application)
        {
            if (!string.IsNullOrEmpty(fallbackCondition))
            {
                if (application.SourceDirectory.DirectoryExists(Path))
                {
                    application.GetDefaultBundleInitializer(GetType())
                               .InitializeBundle(this, application);
                }
                else
                {
                    throw new InvalidOperationException(string.Format("Cannot initialize external bundle's fallback assets. The bundle directory \"{0}\" does not exist and no other assets have been added.", Path));
                }
            }
        }

        public override void Process(ICassetteApplication application)
        {
            // Any fallback assets are processed like a regular ScriptBundle.
            base.Process(application);
            // We just need a special renderer instead.
            externalRenderer = new ExternalScriptBundleHtmlRenderer(Renderer, application);
        }

        public override IHtmlString Render()
        {
            return externalRenderer.Render(this);
        }

        public override bool ContainsPath(string path)
        {
            return base.ContainsPath(path) || url.Equals(path, StringComparison.OrdinalIgnoreCase);
        }
    }
}