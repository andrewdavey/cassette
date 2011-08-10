using System.Web;

namespace Cassette.Web
{
    public class CassetteHttpModule : IHttpModule
    {
        CassetteApplication application;
        
        public void Init(HttpApplication httpApplication)
        {
            this.application = StartUp.CassetteApplication;

            httpApplication.BeginRequest += HttpApplicationBeginRequest;
        }

        void HttpApplicationBeginRequest(object sender, System.EventArgs e)
        {
            // TODO: Install response buffer
            // TODO: Add asset reference helper to HttpContext.Items
        }

        public void Dispose()
        {
        }
    }
}
