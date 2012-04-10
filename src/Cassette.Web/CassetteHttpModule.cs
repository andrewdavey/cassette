using System.Web;

namespace Cassette.Web
{
    /// <summary>
    /// Serves as the entry point into Cassette for an ASP.NET web application.
    /// </summary>
    public class CassetteHttpModule : IHttpModule
    {
        static readonly object Lock = new object();
        static int _initializedModuleCount;
        static WebHost _host;
        internal static string StartUpTrace;

        public void Init(HttpApplication httpApplication)
        {
            lock (Lock)
            {
                var isFirstModuleInitForAppDomain = _initializedModuleCount == 0;
                _initializedModuleCount++;
                if (isFirstModuleInitForAppDomain)
                {
                    using (var recorder = new StartUpTraceRecorder())
                    {
                        _host = new WebHost();
                        _host.Initialize();

                        StartUpTrace = recorder.TraceOutput;
                    }
                }
            }

            _host.Hook(httpApplication);
        }

        public void Dispose()
        {
            lock (Lock)
            {
                _initializedModuleCount--;
                var isFinalModule = _initializedModuleCount == 0;
                if (isFinalModule)
                {
                    _host.Dispose();
                }
            }
        }
    }
}