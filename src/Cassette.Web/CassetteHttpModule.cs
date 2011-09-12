using System;
using System.Web;

namespace Cassette.Web
{
    public class CassetteHttpModule : IHttpModule
    {
        public void Init(HttpApplication httpApplication)
        {
            httpApplication.PostMapRequestHandler += HttpApplicationPostMapRequestHandler;
            httpApplication.PostRequestHandlerExecute += HttpApplicationPostRequestHandlerExecute;
        }

        void HttpApplicationPostMapRequestHandler(object sender, EventArgs e)
        {
            var context = new HttpContextWrapper(((HttpApplication)sender).Context);
            StartUp.CassetteApplication.OnPostMapRequestHandler(context);
        }

        void HttpApplicationPostRequestHandlerExecute(object sender, EventArgs e)
        {
            var context = new HttpContextWrapper(((HttpApplication)sender).Context);
            StartUp.CassetteApplication.OnPostRequestHandlerExecute(context);
        }

        public void Dispose()
        {
        }
    }
}