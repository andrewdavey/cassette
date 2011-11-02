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
using Cassette.Utilities;

namespace Cassette.UI
{
    public class ReferenceBuilder<T> : IReferenceBuilder<T>
        where T: Module
    {
        public ReferenceBuilder(IModuleContainer<T> moduleContainer, IModuleFactory<T> moduleFactory, IPlaceholderTracker placeholderTracker, ICassetteApplication application)
        {
            this.moduleContainer = moduleContainer;
            this.moduleFactory = moduleFactory;
            this.placeholderTracker = placeholderTracker;
            this.application = application;
        }

        readonly IModuleContainer<T> moduleContainer;
        readonly IModuleFactory<T> moduleFactory;
        readonly IPlaceholderTracker placeholderTracker;
        readonly ICassetteApplication application;
        readonly Dictionary<string, List<Module>> modulesByLocation = new Dictionary<string, List<Module>>();
        readonly HashSet<string> renderedLocations = new HashSet<string>();
 
        public void Reference(string path, string location = null)
        {
            path = PathUtilities.AppRelative(path);

            var module = moduleContainer.FindModuleContainingPath(path);
            if (module == null && path.IsUrl())
            {
                // Ad-hoc external module reference.
                module = moduleFactory.CreateExternalModule(path);
            }

            if (module == null)
            {
                throw new ArgumentException("Cannot find an asset module containing the path \"" + path + "\".");                
            }

            // Module can define it's own prefered location. Use this when we aren't given
            // an explicit location argument i.e. null.
            if (location == null)
            {
                location = module.Location;
            }

            Reference(module, location);
        }

        public void Reference(Module module, string location = null)
        {
            if (!application.HtmlRewritingEnabled && HasRenderedLocation(location))
            {
                ThrowRewritingRequiredException(location);
            }

            var modules = GetOrCreateModuleSet(location);
            if (modules.Contains(module)) return;
            modules.Add(module);
        }

        bool HasRenderedLocation(string location)
        {
            return renderedLocations.Contains(location ?? "");
        }

        void ThrowRewritingRequiredException(string location)
        {
            if (string.IsNullOrEmpty(location))
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Cannot add a {0} reference. The modules have already been rendered. Either move the reference before the render call, or set ICassetteApplication.HtmlRewritingEnabled to true in your Cassette configuration.",
                        typeof(T).Name
                    )
                );
            }
            else
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Cannot add a {1} reference, for location \"{0}\". This location has already been rendered. Either move the reference before the render call, or set ICassetteApplication.HtmlRewritingEnabled to true in your Cassette configuration.",
                        location,
                        typeof(T).Name
                    )
                );
            }
        }

        public IEnumerable<Module> GetModules(string location)
        {
            var modules = GetOrCreateModuleSet(location);
            return moduleContainer.IncludeReferencesAndSortModules(modules);
        }

        public string Render(string location = null)
        {
            renderedLocations.Add(location ?? "");
            return placeholderTracker.InsertPlaceholder(
                () => CreateHtml(location)
            );
        }

        public string ModuleUrl(string path)
        {
            var module = moduleContainer.FindModuleContainingPath(path);
            if (module == null)
            {
                throw new ArgumentException("Cannot find module contain path \"" + path + "\".");
            }
            return application.UrlGenerator.CreateModuleUrl(module);
        }

        string CreateHtml(string location)
        {
            return string.Join(Environment.NewLine,
                               GetModules(location).Select(
                               module => module.Render(application)
                              ));
        }

        List<Module> GetOrCreateModuleSet(string location)
        {
            location = location ?? ""; // Dictionary doesn't accept null keys.
            List<Module> modules;
            if (modulesByLocation.TryGetValue(location, out modules))
            {
                return modules;
            }
            else
            {
                modules = new List<Module>();
                modulesByLocation.Add(location, modules);
                return modules;
            }
        }
    }
}
