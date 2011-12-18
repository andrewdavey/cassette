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

namespace Cassette.Configuration
{
    abstract class BundleContainerFactoryBase : IBundleContainerFactory
    {
        protected BundleContainerFactoryBase(IDictionary<Type, IBundleFactory<Bundle>> bundleFactories)
        {
            this.bundleFactories = bundleFactories;
        }

        readonly IDictionary<Type, IBundleFactory<Bundle>> bundleFactories;

        public abstract IBundleContainer Create(IEnumerable<Bundle> unprocessedBundles, CassetteSettings settings);

        protected void ProcessAllBundles(IEnumerable<Bundle> bundles, CassetteSettings settings)
        {
            Trace.Source.TraceInformation("Processing bundles...");
            foreach (var bundle in bundles)
            {
                Trace.Source.TraceInformation("Processing {0} {1}", bundle.GetType().Name, bundle.Path);
                bundle.Process(settings);
            }
            Trace.Source.TraceInformation("Bundle processing completed.");
        }

        protected IEnumerable<Bundle> CreateExternalBundlesFromReferences(IEnumerable<Bundle> bundlesArray, CassetteSettings settings)
        {
            var referencesAlreadyCreated = new HashSet<string>();
            foreach (var bundle in bundlesArray)
            {
                foreach (var reference in bundle.References)
                {
                    if (reference.IsUrl() == false) continue;
                    if (referencesAlreadyCreated.Contains(reference)) continue;

                    var externalBundle = CreateExternalBundle(reference, bundle, settings);
                    referencesAlreadyCreated.Add(externalBundle.Path);
                    yield return externalBundle;
                }
                foreach (var asset in bundle.Assets)
                {
                    foreach (var assetReference in asset.References)
                    {
                        if (assetReference.Type != AssetReferenceType.Url ||
                            referencesAlreadyCreated.Contains(assetReference.Path)) continue;

                        var externalBundle = CreateExternalBundle(assetReference.Path, bundle, settings);
                        referencesAlreadyCreated.Add(externalBundle.Path);
                        yield return externalBundle;
                    }
                }
            }
        }

        Bundle CreateExternalBundle(string reference, Bundle referencer, CassetteSettings settings)
        {
            var bundleFactory = GetBundleFactory(referencer.GetType());
            var externalBundle = bundleFactory.CreateExternalBundle(reference);
            externalBundle.Process(settings);
            return externalBundle;
        }

        IBundleFactory<Bundle> GetBundleFactory(Type bundleType)
        {
            IBundleFactory<Bundle> factory;
            if (bundleFactories.TryGetValue(bundleType, out factory))
            {
                return factory;
            }
            throw new ArgumentException(string.Format("Cannot find bundle factory for {0}", bundleType.FullName));
        }
    }
}
