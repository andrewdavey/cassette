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
using Cassette.Configuration;
using Cassette.Persistence;

namespace Cassette
{
    abstract class CassetteApplicationBase : ICassetteApplication
    {
        protected CassetteApplicationBase(IEnumerable<Bundle> bundles, CassetteSettings settings, string cacheVersion)
        {
            this.settings = settings;
            bundleContainer = CreateBundleContainer(bundles, settings, cacheVersion);
        }

        readonly CassetteSettings settings;
        readonly IBundleContainer bundleContainer;

        public CassetteSettings Settings
        {
            get { return settings; }
        }

        protected internal IBundleContainer BundleContainer
        {
            get { return bundleContainer; }
        }

        public virtual T FindBundleContainingPath<T>(string path)
            where T : Bundle
        {
            return bundleContainer.FindBundleContainingPath<T>(path);
        }

        public IReferenceBuilder GetReferenceBuilder()
        {
            return GetOrCreateReferenceBuilder(CreateReferenceBuilder);
        }

        protected abstract IReferenceBuilder GetOrCreateReferenceBuilder(Func<IReferenceBuilder> create);

        protected abstract IPlaceholderTracker GetPlaceholderTracker();

        public void Dispose()
        {
            bundleContainer.Dispose();
        }

        IReferenceBuilder CreateReferenceBuilder()
        {
            return new ReferenceBuilder(
                bundleContainer,
                settings.BundleFactories,
                GetPlaceholderTracker(),
                settings
            );
        }

        static IBundleContainer CreateBundleContainer(IEnumerable<Bundle> bundles, CassetteSettings settings, string cacheVersion)
        {
            IBundleContainerFactory containerFactory;
            if (settings.IsDebuggingEnabled)
            {
                containerFactory = new BundleContainerFactory(settings.BundleFactories);
            }
            else
            {
                containerFactory = new CachedBundleContainerFactory(
                    new BundleCache(
                        cacheVersion,
                        settings
                    ),
                    settings.BundleFactories
                );
            }
            return containerFactory.Create(bundles, settings);
        }

        protected IPlaceholderTracker CreatePlaceholderTracker()
        {
            if (Settings.IsHtmlRewritingEnabled)
            {
                return new PlaceholderTracker();
            }
            else
            {
                return new NullPlaceholderTracker();
            }
        }
    }
}