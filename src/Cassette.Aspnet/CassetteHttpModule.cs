using System.Diagnostics;
using System.Web;

namespace Cassette.Aspnet
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

        /// <summary>
        /// Gets the <see cref="WebHost"/> for the web application.
        /// </summary>
        public static WebHost Host
        {
            get { return _host; }
        }

        public void Init(HttpApplication httpApplication)
        {
            lock (Lock)
            {
                var isFirstModuleInitForAppDomain = _initializedModuleCount == 0;
                _initializedModuleCount++;
                if (isFirstModuleInitForAppDomain)
                {
                    var startupTimer = Stopwatch.StartNew();
                    using (var recorder = new StartUpTraceRecorder())
                    {
                        _host = new WebHost();
                        _host.Initialize();

                        Trace.Source.TraceInformation("Total time elapsed: {0}ms", startupTimer.ElapsedMilliseconds);
                        StartUpTrace = recorder.TraceOutput;
                    }
                }
            }

            HandleHttpApplicationEvents(httpApplication);
        }

        void HandleHttpApplicationEvents(HttpApplication httpApplication)
        {
            var rewriter = _host.CreatePlaceholderRewriter();
            httpApplication.BeginRequest += (s, e) => Host.StoreRequestContainerInHttpContextItems();
            httpApplication.EndRequest += (s, e) => Host.RemoveRequestContainerFromHttpContextItems();

            httpApplication.PostAuthorizeRequest += (s, e) => RewriteFileRequests();

            httpApplication.PostMapRequestHandler += (s, e) => rewriter.AddPlaceholderTrackerToHttpContextItems();
            httpApplication.PostRequestHandlerExecute += (s, e) => rewriter.RewriteOutput();
        }

        void RewriteFileRequests()
        {
            var rewriter = Host.RequestContainer.Resolve<RawFileRequestRewriter>();
            rewriter.Rewrite();
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