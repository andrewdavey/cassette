using System;
using System.Collections.Generic;
using Cassette.Configuration;

namespace Cassette
{
    abstract class CassetteApplicationBase : ICassetteApplication
    {
        protected CassetteApplicationBase(IEnumerable<Bundle> bundles, CassetteSettings settings)
        {
            this.settings = settings;
            bundleContainer = CreateBundleContainer(bundles);
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

        IBundleContainer CreateBundleContainer(IEnumerable<Bundle> bundles)
        {
            var factory = settings.GetBundleContainerFactory();
            return factory.Create(bundles, settings);
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