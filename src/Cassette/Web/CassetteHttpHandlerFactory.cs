using System;
using System.Web;

namespace Cassette.Web
{
    public class CassetteHttpHandlerFactory : IHttpHandlerFactory
    {
        public IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
        {
            return SingleManagerContainer.Manager.CreateHttpHandler();
        }

        public void ReleaseHandler(IHttpHandler handler)
        {
            var disposable = handler as IDisposable;
            if (disposable != null) disposable.Dispose();
        }
    }
}
