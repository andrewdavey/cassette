using System.Web;

namespace Cassette.Web
{
    public class CassetteHttpModule : IHttpModule
    {
        static readonly object Lock = new object();
        static int _initializedModuleCount;
        internal static string StartUpTrace;

        public void Init(HttpApplication httpApplication)
        {
            lock (Lock)
            {
                var isFirstModuleInitForAppDomain = _initializedModuleCount == 0;
                _initializedModuleCount++;
                if (!isFirstModuleInitForAppDomain) return;

                using (var recorder = new StartUpTraceRecorder())
                {
                    var bootstrapper = BootstrapperLocator<DefaultBootstrapper>.Bootstrapper;
                    bootstrapper.Initialize();

                    StartUpTrace = recorder.TraceOutput;
                }
            }
        }

        void IHttpModule.Dispose()
        {
            lock (Lock)
            {
                _initializedModuleCount--;
                var isFinalModule = _initializedModuleCount == 0;
                if (!isFinalModule) return;

                Shutdown();
            }
        }

        void Shutdown()
        {
            IsolatedStorageContainer.Dispose();
            
        }
    }
}