using System;
using System.Web;

namespace Cassette.Web
{
    public class CassetteHttpModule : IHttpModule
    {
#if NET35
        private static object _lock = new object();
        private static int _initializedModuleCount;
#endif

        public void Init(HttpApplication httpApplication)
        {
            httpApplication.PostMapRequestHandler += HttpApplicationPostMapRequestHandler;
            httpApplication.PostRequestHandlerExecute += HttpApplicationPostRequestHandlerExecute;            
#if NET35
            // FX35: Handle app_start, app_end events to avoid forcing folks to edit their Global asax files
            // See: https://bitbucket.org/davidebbo/webactivator/src/bb05d55459bc/WebActivator/ActivationManager.cs#cl-121
            lock (_lock)
            {
                // Keep track of the number of modules initialized and
                // make sure we only call the startup methods once per app domain
                if (_initializedModuleCount++ != 0) return;

                StartUp.PreApplicationStart();
                StartUp.PostApplicationStart();
            }
#endif
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
#if NET35
            lock (_lock)
            {
                // Call the shutdown methods when the last module is disposed
                if (--_initializedModuleCount != 0) return;

                StartUp.ApplicationShutdown();
            }
#endif
        }
    }
}
