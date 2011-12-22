using System;
using System.Web;

namespace Cassette.Web
{
    public class CassetteHttpModule : IHttpModule
    {
        CassetteApplication Application
        {
            get { return (CassetteApplication)CassetteApplicationContainer.Instance.Application; }
        }

        public void Init(HttpApplication httpApplication)
        {
            httpApplication.PostMapRequestHandler += HttpApplicationPostMapRequestHandler;
            httpApplication.PostRequestHandlerExecute += HttpApplicationPostRequestHandlerExecute;
        }

        void HttpApplicationPostMapRequestHandler(object sender, EventArgs e)
        {
            Application.OnPostMapRequestHandler();
        }

        void HttpApplicationPostRequestHandlerExecute(object sender, EventArgs e)
        {
            Application.OnPostRequestHandlerExecute();
        }

        void IHttpModule.Dispose()
        {
        }
    }
}
