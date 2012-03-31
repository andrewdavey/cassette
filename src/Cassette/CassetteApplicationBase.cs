using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.Configuration;

namespace Cassette
{
    abstract class CassetteApplicationBase : ICassetteApplication
    {
        protected CassetteApplicationBase(IBundleContainer bundleContainer, IBundleFactoryProvider bundleFactoryProvider, CassetteSettings settings)
        {
            this.settings = settings;
            this.bundleContainer = bundleContainer;
            this.bundleFactoryProvider = bundleFactoryProvider;
        }

        readonly CassetteSettings settings;
        readonly IBundleContainer bundleContainer;
        readonly IBundleFactoryProvider bundleFactoryProvider;

        public CassetteSettings Settings
        {
            get { return settings; }
        }

        public IEnumerable<Bundle> Bundles
        {
            get { return bundleContainer.Bundles; }
        }

        public virtual T FindBundleContainingPath<T>(string path)
            where T : Bundle
        {
            return bundleContainer.FindBundlesContainingPath(path).OfType<T>().FirstOrDefault();
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
                GetPlaceholderTracker(),
                bundleFactoryProvider,
                settings
            );
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