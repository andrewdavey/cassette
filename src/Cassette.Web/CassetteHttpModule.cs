using System;
using System.Web;

namespace Cassette.Web
{
    public class CassetteHttpModule : IHttpModule
    {
        public void Init(HttpApplication httpApplication)
        {
            httpApplication.BeginRequest += HttpApplicationBeginRequest;
        }

        void HttpApplicationBeginRequest(object sender, EventArgs e)
        {
            var context = new HttpContextWrapper(((HttpApplication)sender).Context);
            StartUp.CassetteApplication.OnBeginRequest(context);
        }

        public void Dispose()
        {
        }
    }
}