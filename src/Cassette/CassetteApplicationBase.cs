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
            moduleFactories = CreateModuleFactories();
            moduleContainers = CreateModuleContainers(
                configurations,
                cacheDirectory,
                CombineVersionWithCassetteVersion(version)
            );
        }

        bool isOutputOptimized;
        readonly IDirectory rootDirectory;
        IUrlGenerator urlGenerator;
        readonly Dictionary<Type, IModuleContainer<Module>> moduleContainers;
        readonly Dictionary<Type, object> moduleFactories;

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

        public IReferenceBuilder<T> GetReferenceBuilder<T>() where T : Module
        {
            return GetOrCreateReferenceBuilder(CreateReferenceBuilder<T>);
        }

        protected abstract IReferenceBuilder<T> GetOrCreateReferenceBuilder<T>(Func<IReferenceBuilder<T>> create) where T : Module;

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            foreach(var container in moduleContainers.Values)
            {
                container.Dispose();
            }
            GC.SuppressFinalize(this); 
        }

        IReferenceBuilder<T> CreateReferenceBuilder<T>()
            where T : Module
        {
            return new ReferenceBuilder<T>(
                GetModuleContainer<T>(),
                (IModuleFactory<T>)moduleFactories[typeof(T)],
                GetPlaceholderTracker(),
                this
            );
        }

        protected abstract IPlaceholderTracker GetPlaceholderTracker();

        protected IModuleContainer<T> GetModuleContainer<T>()
            where T: Module
        {
            IModuleContainer<Module> container;
            if (moduleContainers.TryGetValue(typeof(T), out container))
            {
                return (IModuleContainer<T>)container;
            }
            else
            {
                return new ModuleContainer<T>(Enumerable.Empty<T>());
            }
        }

        public Module FindModuleContainingPath(string path)
        {
            return moduleContainers.Values
                .Select(container => container.FindModuleContainingPath(path))
                .FirstOrDefault(module => module != null);
        }

        Dictionary<Type, IModuleContainer<Module>> CreateModuleContainers(IEnumerable<ICassetteConfiguration> configurations, IDirectory cacheDirectory, string version)
        {
            var moduleConfiguration = new ModuleConfiguration(this, cacheDirectory, rootDirectory, moduleFactories, version);
            foreach (var configuration in configurations)
            {
                configuration.Configure(moduleConfiguration, this);
            }
            AddDefaultModuleSourcesIfEmpty(moduleConfiguration);
            return moduleConfiguration.CreateModuleContainers(isOutputOptimized, version);
        }

        Dictionary<Type, object> CreateModuleFactories()
        {
            return new Dictionary<Type, object>
            {
                { typeof(ScriptModule), new ScriptModuleFactory() },
                { typeof(StylesheetModule), new StylesheetModuleFactory() },
                { typeof(HtmlTemplateModule), new HtmlTemplateModuleFactory() }
            };
        }

        /// <remarks>
        /// We need module container cache to depend on both the application version
        /// and the Cassette version. So if either is upgraded, then the cache is discarded.
        /// </remarks>
        string CombineVersionWithCassetteVersion(string version)
        {
            return version + "|" + GetType().Assembly.GetName().Version;
        }

        void AddDefaultModuleSourcesIfEmpty(ModuleConfiguration moduleConfiguration)
        {
            if (moduleConfiguration.ContainsModuleSources(typeof(ScriptModule)) == false)
            {
                moduleConfiguration.Add(DefaultScriptModuleSource());
            }
            if (moduleConfiguration.ContainsModuleSources(typeof(StylesheetModule)) == false)
            {
                moduleConfiguration.Add(DefaultStylesheetModuleSource());
            }
            if (moduleConfiguration.ContainsModuleSources(typeof(HtmlTemplateModule)) == false)
            {
                moduleConfiguration.Add(DefaultHtmlTemplateModuleSource());
            }
        }

        IModuleSource<ScriptModule> DefaultScriptModuleSource()
        {
            return new PerFileSource<ScriptModule>("")
            {
                FilePattern = "*.js;*.coffee",
                Exclude = new Regex("-vsdoc\\.js$")
            };
        }

        IModuleSource<StylesheetModule> DefaultStylesheetModuleSource()
        {
            return new PerFileSource<StylesheetModule>("")
            {
                FilePattern = "*.css;*.less"
            };
        }

        IModuleSource<HtmlTemplateModule> DefaultHtmlTemplateModuleSource()
        {
            return new PerFileSource<HtmlTemplateModule>("")
            {
                FilePattern = "*.htm;*.html"
            };
        }
    }
}
