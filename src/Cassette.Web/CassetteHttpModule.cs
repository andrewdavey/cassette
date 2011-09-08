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

            // Fix for dealing with empty responses for ScriptResource.axd and WebResource.axd files
            // http://forums.asp.net/t/1550676.aspx/1
            if (context.Request.Path.ToLower().Contains(".axd"))
                return;
            
            StartUp.CassetteApplication.OnBeginRequest(context);
        }

        public void Dispose()
        {
        }
    }
}