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
using Cassette.IO;
using Cassette.Persistence;
using Cassette.UI;

namespace Cassette
{
    public abstract class CassetteApplicationBase : ICassetteApplication
    {
        protected CassetteApplicationBase(IEnumerable<Bundle> bundles, CassetteSettings settings, IUrlGenerator urlGenerator)
        {
            this.settings = settings;
            this.urlGenerator = urlGenerator;

            // Bundle container must be created after the above fields are assigned.
            // This application object may get used during bundle processing, so its properties must be ready to use.
            bundleContainer = CreateBundleContainer(bundles, settings, this);
        }

        readonly CassetteSettings settings;
        readonly IUrlGenerator urlGenerator;
        readonly IBundleContainer bundleContainer;

        public bool IsDebuggingEnabled
        {
            get { return settings.IsDebuggingEnabled; }
        }

        public bool IsHtmlRewritingEnabled
        {
            get { return settings.IsHtmlRewritingEnabled; }
        }

        public IDirectory SourceDirectory
        {
            get { return settings.SourceDirectory; }
        }

        public IUrlGenerator UrlGenerator
        {
            get { return urlGenerator; }
        }

        protected IBundleContainer BundleContainer
        {
            get { return bundleContainer; }
        }

        public IReferenceBuilder<T> GetReferenceBuilder<T>() where T : Bundle
        {
            return GetOrCreateReferenceBuilder(CreateReferenceBuilder<T>);
        }

        protected abstract IReferenceBuilder<T> GetOrCreateReferenceBuilder<T>(Func<IReferenceBuilder<T>> create) where T : Bundle;

        protected abstract IPlaceholderTracker GetPlaceholderTracker();

        public void Dispose()
        {
            bundleContainer.Dispose();
        }

        IReferenceBuilder<T> CreateReferenceBuilder<T>()
            where T : Bundle
        {
            return new ReferenceBuilder<T>(
                bundleContainer,
                settings.BundleFactories[typeof(T)],
                GetPlaceholderTracker(),
                this
            );
        }

        static IBundleContainer CreateBundleContainer(IEnumerable<Bundle> bundles, CassetteSettings settings, ICassetteApplication application)
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
                        settings.CacheVersion,
                        settings.CacheDirectory,
                        settings.SourceDirectory
                    ),
                    settings.BundleFactories
                );
            }
            return containerFactory.Create(bundles, application);
        }
    }
}