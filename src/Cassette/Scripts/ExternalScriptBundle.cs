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
using Cassette.Utilities;

namespace Cassette.Scripts
{
    public class ExternalScriptBundle : ScriptBundle
    {
        public ExternalScriptBundle(string url)
            : this(url, url)
        {
        }

        public ExternalScriptBundle(string path, BundleDescriptor bundleDescriptor)
            : base(path, bundleDescriptor)
        {
            if (bundleDescriptor == null)
            {
                url = path;
            }
            else
            {
                url = bundleDescriptor.ExternalUrl;
                javaScriptFallbackCondition = bundleDescriptor.FallbackCondition;
            }
        }

        public ExternalScriptBundle(string name, string url)
            : base(PathUtilities.AppRelative(name))
        {
            if (url == null) throw new ArgumentNullException("url");
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("URL is required.", "url");

            this.url = url;

            BundleInitializers.Clear();
        }

        public ExternalScriptBundle(string name, string url, string javaScriptFallbackCondition, string fallbackUrl)
            : base(PathUtilities.AppRelative(name))
        {
            if (url == null) throw new ArgumentNullException("url");
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("URL is required.", "url");
            if (javaScriptFallbackCondition == null) throw new ArgumentNullException("javaScriptFallbackCondition");
            if (string.IsNullOrWhiteSpace(javaScriptFallbackCondition)) throw new ArgumentException("JavaScript condition is required.", "javaScriptFallbackCondition");
            if (fallbackUrl == null) throw new ArgumentNullException("fallbackUrl");
            if (string.IsNullOrWhiteSpace(fallbackUrl)) throw new ArgumentException("Fallback URL is required.", "fallbackUrl");
            
            this.url = url;
            this.javaScriptFallbackCondition = javaScriptFallbackCondition;
            this.fallbackUrl = PathUtilities.AppRelative(fallbackUrl);

            BundleInitializers.Clear();
        }

        readonly string url;
        string javaScriptFallbackCondition;
        // TODO: remove this field?
        readonly string fallbackUrl;

        public string Url
        {
            get { return url; }
        }

        public string FallbackCondition
        {
            get { return javaScriptFallbackCondition; }
        }

        public void AddFallback(string javaScriptFallbackCondition, IEnumerable<IAsset> fallbackAssets)
        {
            this.javaScriptFallbackCondition = javaScriptFallbackCondition;
            AddAssets(fallbackAssets, true);
        }

        public override void Process(ICassetteApplication application)
        {
            if (string.IsNullOrEmpty(fallbackUrl) == false)
            {
                Assets.Add(new Asset(fallbackUrl, this, application.RootDirectory.GetFile(fallbackUrl.Substring(2))));
            }
            base.Process(application);
        }

        public override IHtmlString Render(ICassetteApplication application)
        {
            var externalRenderer = new ExternalScriptBundleHtmlRenderer(Renderer, application);
            return externalRenderer.Render(this);
        }

        public override bool ContainsPath(string path)
        {
            return base.ContainsPath(path) || url.Equals(path, StringComparison.OrdinalIgnoreCase);
        }
    }
}
