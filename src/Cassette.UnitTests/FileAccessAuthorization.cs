using System;
using Cassette.Configuration;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class FileAccessAuthorization_Tests
    {
        readonly FileAccessAuthorization authorization;
        readonly Mock<IConfiguration<IFileAccessAuthorization>> configuration;
        readonly BundleCollection bundles;

        public FileAccessAuthorization_Tests()
        {
            configuration = new Mock<IConfiguration<IFileAccessAuthorization>>();
            var configurations = new[] { configuration.Object };
            bundles = new BundleCollection(new CassetteSettings(), Mock.Of<IFileSearchProvider>(), Mock.Of<IBundleFactoryProvider>());

            authorization = new FileAccessAuthorization(configurations, bundles);
        }

        [Fact]
        public void ConfigurationsAppliedByConstructor()
        {
            configuration.Verify(c => c.Configure(authorization));
        }

        [Fact]
        public void GivenAllowAccessToPath_ThenCanAccessPathReturnsTrue()
        {
            authorization.AllowAccess("~/test.png");
            authorization.CanAccess("~/test.png").ShouldBeTrue();
        }

        [Fact]
        public void GivenNoPathsAllowed_ThenCanAccessPathReturnsFalse()
        {
            authorization.CanAccess("~/test.png").ShouldBeFalse();
        }

        [Fact]
        public void GivenAllowAccessToPath_ThenCanAccessPathWithDifferentCasingReturnsTrue()
        {
            authorization.AllowAccess("~/test.png");
            authorization.CanAccess("~/TEST.PNG").ShouldBeTrue();
        }

        [Fact]
        public void GivenAllowAccessToPathPredicate_ThenCanAccessPathMatchingPredicateReturnsTrue()
        {
            authorization.AllowAccess(path => path == "~/test.png");
            authorization.CanAccess("~/test.png").ShouldBeTrue();
        }

        [Fact]
        public void GivenBundleWithAssetWithRawFileReference_ThenCanAccessTheRawFilePath()
        {
            var bundle = new TestableBundle("~");
            var asset = new StubAsset("~/test.css");
            asset.AddRawFileReference("~/test.png");
            bundle.Assets.Add(asset);
            bundles.Add(bundle);

            authorization.CanAccess("~/test.png").ShouldBeTrue();
        }

        [Fact]
        public void AllowAccessPathCannotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => authorization.AllowAccess((string)null));
        }

        [Fact]
        public void AllowAccessPathPredicateCannotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => authorization.AllowAccess((Func<string, bool>)null));
        }

        [Fact]
        public void CanAccessWithNullPathReturnsFalseWithoutTestingPredicates()
        {
            authorization.AllowAccess(path => path.StartsWith("test"));
            authorization.CanAccess(null).ShouldBeFalse();
        }

        [Fact]
        public void AllowAccessPathMustBeApplicationRelative()
        {
            Assert.Throws<ArgumentException>(() => authorization.AllowAccess("incorrect-path"));
        }
    }
}