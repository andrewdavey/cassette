using System;
using System.Web;

namespace Cassette.Web
{
    public class CassetteHttpModule : IHttpModule
    {
        static readonly object Lock = new object();
        static int _initializedModuleCount;

        public void Init(HttpApplication httpApplication)
        {
            httpApplication.PostMapRequestHandler += HttpApplicationPostMapRequestHandler;
            httpApplication.PostRequestHandlerExecute += HttpApplicationPostRequestHandlerExecute;            

            // FX35 & FX40: Handle app_start, app_end events to avoid forcing folks to edit their Global asax files
            // See: https://bitbucket.org/davidebbo/webactivator/src/bb05d55459bc/WebActivator/ActivationManager.cs#cl-121
            lock (Lock)
            {
                // Keep track of the number of modules initialized and
                // make sure we only call the startup methods once per app domain
                if (_initializedModuleCount++ != 0) return;

                StartUp.ApplicationStart();
            }
        }

        void HttpApplicationPostMapRequestHandler(object sender, EventArgs e)
        {
            Application.OnPostMapRequestHandler();
        }

        void HttpApplicationPostRequestHandlerExecute(object sender, EventArgs e)
        {
            Application.OnPostRequestHandlerExecute();
        }

        CassetteApplication Application
        {
            get { return (CassetteApplication)CassetteApplicationContainer.Application; }
        }

        void IHttpModule.Dispose()
        {
            lock (Lock)
            {
                // Call the shutdown methods when the last module is disposed
                if (--_initializedModuleCount != 0) return;

                StartUp.ApplicationShutdown();
            }
        }
    }
}
