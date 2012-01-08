using System;
using System.Web;
using Cassette.Configuration;

namespace Cassette.Web
{
    class PreBuiltCassetteApplication : ICassetteApplication
    {
        readonly Func<HttpContextBase> getCurrentHttpContext;
        readonly CassetteSettings settings;

        public PreBuiltCassetteApplication(string appBundlesPath, Func<HttpContextBase> getCurrentHttpContext)
        {
            this.getCurrentHttpContext = getCurrentHttpContext;
            settings = new CassetteSettings("")
            {
                BundlesArePreBuilt = true
            };
        }

        public CassetteSettings Settings
        {
            get { return settings; }
        }

        public T FindBundleContainingPath<T>(string path) where T : Bundle
        {
            throw new NotImplementedException();
        }

        public IReferenceBuilder GetReferenceBuilder()
        {
            return GetOrCreateReferenceBuilder();
        }

        IReferenceBuilder GetOrCreateReferenceBuilder()
        {
            const string key = "Cassette.ReferenceBuilder";
            var items = getCurrentHttpContext().Items;
            if (items.Contains(key))
            {
                return (IReferenceBuilder)items[key];
            }
            else
            {
                var builder = new ReferenceBuilder(bundleContainer, settings.BundleFactories, new PlaceholderTracker(),
                                                   Settings);
                items[key] = builder;
                return builder;
            }
        }

        public void Dispose()
        {
            
        }
    }
}
