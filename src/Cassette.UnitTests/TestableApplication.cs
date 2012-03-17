using System;
using System.Linq;
using Cassette.Configuration;
using Cassette.IO;
using Moq;

namespace Cassette
{
    class TestableApplication : CassetteApplicationBase
    {
        readonly IReferenceBuilder referenceBuilder;
        readonly IBundleContainer bundleContainer;

        public TestableApplication(IUrlGenerator urlGenerator = null, IReferenceBuilder referenceBuilder = null, IBundleContainer bundleContainer = null, IDirectory sourceDirectory = null)
            : base(
                bundleContainer ?? new BundleContainer(Enumerable.Empty<Bundle>()), 
                new CassetteSettings("")
                {
                    SourceDirectory = sourceDirectory ?? Mock.Of<IDirectory>(),
                    IsDebuggingEnabled = true,
                    UrlGenerator = urlGenerator ?? Mock.Of<IUrlGenerator>()
                }
            )
        {
            this.referenceBuilder = referenceBuilder;
            this.bundleContainer = bundleContainer;
        }

        public override T FindBundleContainingPath<T>(string path)
        {
            return bundleContainer.FindBundlesContainingPath(path).OfType<T>().FirstOrDefault();
        }
            
        protected override IReferenceBuilder GetOrCreateReferenceBuilder(Func<IReferenceBuilder> create)
        {
            return referenceBuilder;
        }

        protected override IPlaceholderTracker GetPlaceholderTracker()
        {
            throw new NotImplementedException();
        }
    }
}