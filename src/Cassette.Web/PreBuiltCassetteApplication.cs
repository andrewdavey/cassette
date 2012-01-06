using System;
using System.Web;
using Cassette.Configuration;

namespace Cassette
{
    class PreBuiltCassetteApplication : ICassetteApplication
    {
        readonly Func<HttpContextBase> getCurrentHttpContext;

        public PreBuiltCassetteApplication(string preBuiltBundlesManifestFilename, Func<HttpContextBase> getCurrentHttpContext)
        {
            this.getCurrentHttpContext = getCurrentHttpContext;
        }

        public CassetteSettings Settings
        {
            get { throw new NotImplementedException(); }
        }

        public T FindBundleContainingPath<T>(string path) where T : Bundle
        {
            throw new NotImplementedException();
        }

        public IReferenceBuilder GetReferenceBuilder()
        {
            throw new NotImplementedException();
        }
        
        public void Dispose()
        {
            
        }
    }
}
