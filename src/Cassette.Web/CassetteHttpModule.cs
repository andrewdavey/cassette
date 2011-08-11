using System;
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

        void HttpApplicationBeginRequest(object sender, EventArgs e)
        {
            // TODO: Install response buffer
            //((HttpApplication)sender).Response.Filter = 
        }

        public void Dispose()
        {
        }
    }
}
