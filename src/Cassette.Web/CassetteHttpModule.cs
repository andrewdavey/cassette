using System;
using System.Web;

namespace Cassette.Web
{
    public class CassetteHttpModule : IHttpModule
    {
        public void Init(HttpApplication httpApplication)
        {
            httpApplication.BeginRequest += HttpApplicationBeginRequest;
            httpApplication.PostRequestHandlerExecute += HttpApplicationPostRequestHandlerExecute;
        }

        void HttpApplicationBeginRequest(object sender, EventArgs e)
        {
            var context = new HttpContextWrapper(((HttpApplication)sender).Context);
            StartUp.CassetteApplication.OnBeginRequest(context);
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