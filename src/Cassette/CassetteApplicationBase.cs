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
using System.Text.RegularExpressions;
using Cassette.HtmlTemplates;
using Cassette.IO;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Cassette.UI;

namespace Cassette
{
    public abstract class CassetteApplicationBase : ICassetteApplication
    {
        protected CassetteApplicationBase(IEnumerable<ICassetteConfiguration> configurations, IDirectory rootDirectory, IDirectory cacheDirectory, IUrlGenerator urlGenerator, bool isOutputOptimized, string version)
        {
            this.rootDirectory = rootDirectory;
            this.isOutputOptimized = isOutputOptimized;
            this.urlGenerator = urlGenerator;
            HtmlRewritingEnabled = true;
            bundleFactories = CreateBundleFactories();
            bundleContainers = CreateBundleContainers(
                configurations,
                cacheDirectory,
                CombineVersionWithCassetteVersion(version)
            );
        }

        bool isOutputOptimized;
        readonly IDirectory rootDirectory;
        IUrlGenerator urlGenerator;
        readonly Dictionary<Type, IBundleContainer<Bundle>> bundleContainers;
        readonly Dictionary<Type, object> bundleFactories;

        public bool IsOutputOptimized
        {
            get { return isOutputOptimized; }
            set { isOutputOptimized = value; }
        }

        public IDirectory RootDirectory
        {
            get { return rootDirectory; }
        }

        public IUrlGenerator UrlGenerator
        {
            get { return urlGenerator; }
            set
            {
                if (value == null) throw new ArgumentNullException("value", "UrlGenerator cannot be null.");
                urlGenerator = value;
            }
        }

        public bool HtmlRewritingEnabled { get; set; }

        public IReferenceBuilder<T> GetReferenceBuilder<T>() where T : Bundle
        {
            return GetOrCreateReferenceBuilder(CreateReferenceBuilder<T>);
        }

        protected abstract IReferenceBuilder<T> GetOrCreateReferenceBuilder<T>(Func<IReferenceBuilder<T>> create) where T : Bundle;

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            foreach(var container in bundleContainers.Values)
            {
                container.Dispose();
            }
            GC.SuppressFinalize(this); 
        }

        IReferenceBuilder<T> CreateReferenceBuilder<T>()
            where T : Bundle
        {
            return new ReferenceBuilder<T>(
                GetBundleContainer<T>(),
                (IBundleFactory<T>)bundleFactories[typeof(T)],
                GetPlaceholderTracker(),
                this
            );
        }

        protected abstract IPlaceholderTracker GetPlaceholderTracker();

        protected IBundleContainer<T> GetBundleContainer<T>()
            where T: Bundle
        {
            IBundleContainer<Bundle> container;
            if (bundleContainers.TryGetValue(typeof(T), out container))
            {
                return (IBundleContainer<T>)container;
            }
            else
            {
                return new BundleContainer<T>(Enumerable.Empty<T>());
            }
        }

        public Bundle FindBundleContainingPath(string path)
        {
            return bundleContainers.Values
                .Select(container => container.FindBundleContainingPath(path))
                .FirstOrDefault(bundle => bundle != null);
        }

        Dictionary<Type, IBundleContainer<Bundle>> CreateBundleContainers(IEnumerable<ICassetteConfiguration> configurations, IDirectory cacheDirectory, string version)
        {
            var bundleConfiguration = new BundleConfiguration(this, cacheDirectory, rootDirectory, bundleFactories, version);
            foreach (var configuration in configurations)
            {
                configuration.Configure(bundleConfiguration, this);
            }
            AddDefaultBundleSourcesIfEmpty(bundleConfiguration);
            return bundleConfiguration.CreateBundleContainers(isOutputOptimized, version);
        }

        Dictionary<Type, object> CreateBundleFactories()
        {
            return new Dictionary<Type, object>
            {
                { typeof(ScriptBundle), new ScriptBundleFactory() },
                { typeof(StylesheetBundle), new StylesheetBundleFactory() },
                { typeof(HtmlTemplateBundle), new HtmlTemplateBundleFactory() }
            };
        }

        /// <remarks>
        /// We need bundle container cache to depend on both the application version
        /// and the Cassette version. So if either is upgraded, then the cache is discarded.
        /// </remarks>
        string CombineVersionWithCassetteVersion(string version)
        {
            return version + "|" + GetType().Assembly.GetName().Version;
        }

        void AddDefaultBundleSourcesIfEmpty(BundleConfiguration bundleConfiguration)
        {
            if (bundleConfiguration.ContainsBundleSources(typeof(ScriptBundle)) == false)
            {
                bundleConfiguration.Add(DefaultScriptBundleSource());
            }
            if (bundleConfiguration.ContainsBundleSources(typeof(StylesheetBundle)) == false)
            {
                bundleConfiguration.Add(DefaultStylesheetBundleSource());
            }
            if (bundleConfiguration.ContainsBundleSources(typeof(HtmlTemplateBundle)) == false)
            {
                bundleConfiguration.Add(DefaultHtmlTemplateBundleSource());
            }
        }

        IBundleSource<ScriptBundle> DefaultScriptBundleSource()
        {
            return new PerFileSource<ScriptBundle>("")
            {
                FilePattern = "*.js;*.coffee",
                Exclude = new Regex("-vsdoc\\.js$")
            };
        }

        IBundleSource<StylesheetBundle> DefaultStylesheetBundleSource()
        {
            return new PerFileSource<StylesheetBundle>("")
            {
                FilePattern = "*.css;*.less"
            };
        }

        IBundleSource<HtmlTemplateBundle> DefaultHtmlTemplateBundleSource()
        {
            return new PerFileSource<HtmlTemplateBundle>("")
            {
                FilePattern = "*.htm;*.html"
            };
        }
    }
}
