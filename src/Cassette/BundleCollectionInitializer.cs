using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.Caching;

namespace Cassette
{
    class BundleCollectionInitializer : IBundleCollectionInitializer
    {
        readonly IEnumerable<IConfiguration<BundleCollection>> bundleConfigurations;
        readonly ExternalBundleGenerator externalBundleGenerator;
        readonly Func<Type, IAssetCacheValidator> createValidator;
        readonly CassetteSettings settings;
        BundleCollection bundles;

        public BundleCollectionInitializer(IEnumerable<IConfiguration<BundleCollection>> bundleConfigurations, ExternalBundleGenerator externalBundleGenerator, Func<Type, IAssetCacheValidator> createValidator, CassetteSettings settings)
        {
            this.bundleConfigurations = bundleConfigurations;
            this.externalBundleGenerator = externalBundleGenerator;
            this.createValidator = createValidator;
            this.settings = settings;
        }

        DateTime? lastInitializationDateTime;

        public void Initialize(BundleCollection bundleCollection)
        {
            using (bundleCollection.GetWriteLock())
            {
                bundles = bundleCollection;
                if (lastInitializationDateTime.HasValue)
                {
                    ReInitialize();
                }
                else
                {
                    FirstInitialize();
                }
            }
        }

        void FirstInitialize()
        {
            bundles.Clear();
            AddBundles();
            bundles.Process();
            bundles.AddRange(externalBundleGenerator.AddBundlesForUrlReferences(bundles));
            bundles.BuildReferences();
            lastInitializationDateTime = DateTime.UtcNow;
        }

        void ReInitialize()
        {
            var upToDateBundles = bundles.Where(BundleIsUpToDate).ToArray();

            bundles.Clear();
            AddBundles();
            var newBundles = bundles.Except(upToDateBundles, new BundleComparer()).ToArray();
            
            foreach (var newBundle in newBundles)
            {
                newBundle.Process(settings);
            }

            var externalBundles = externalBundleGenerator.AddBundlesForUrlReferences(newBundles);

            var all = newBundles.Concat(upToDateBundles).Concat(externalBundles);
            bundles.Clear();
            bundles.AddRange(all);

            bundles.BuildReferences();

            lastInitializationDateTime = DateTime.UtcNow;
        }

        bool BundleIsUpToDate(Bundle bundle)
        {
            if (!lastInitializationDateTime.HasValue) throw new InvalidOperationException();
            if (!bundle.IsProcessed) return false;

            var validator = new AssetValidator(lastInitializationDateTime.Value, createValidator);
            bundle.Accept(validator);
            return validator.IsValid;
        }

        void AddBundles()
        {
            bundleConfigurations.Configure(bundles);
        }

        class AssetValidator : IBundleVisitor
        {
            readonly DateTime asOf;
            readonly Func<Type, IAssetCacheValidator> createValidator;

            public AssetValidator(DateTime asOf, Func<Type, IAssetCacheValidator> createValidator)
            {
                this.asOf = asOf;
                this.createValidator = createValidator;
                IsValid = true;
            }

            public bool IsValid { get; set; }

            public void Visit(Bundle bundle)
            {
            }

            public void Visit(IAsset asset)
            {
                if (!IsValid) return;

                var validator = createValidator(asset.AssetCacheValidatorType);
                if (!validator.IsValid(asset.Path, asOf))
                {
                    IsValid = false;
                }
            }
        }

        class BundleComparer : IEqualityComparer<Bundle>
        {
            public bool Equals(Bundle x, Bundle y)
            {
                return x.Path == y.Path && x.GetType() == y.GetType();
            }

            public int GetHashCode(Bundle obj)
            {
                return new { obj.Path, Type = obj.GetType() }.GetHashCode();
            }
        }
    }
}