using System;
using System.Diagnostics;
using System.Threading;
using System.Web;

namespace Cassette.Spriting.Spritastic.Utilities
{
    static class Tracer
    {
        [Conditional("DEBUG")]
        public static void Trace(string messageFormat, params object[] args)
        {
            if (TrustLevelChecker.GetCurrentTrustLevel() == AspNetHostingPermissionLevel.Unrestricted)
                LogImplementation(messageFormat, args);
        }

        private static void LogImplementation(string messageFormat, params object[] args)
        {
            if (args.Length == 0) messageFormat = messageFormat.Replace("{", "{{").Replace("}", "}}");
            if (System.Diagnostics.Trace.Listeners.Count <= 0) return;
            var msg = string.Format(messageFormat, args);
            System.Diagnostics.Trace.TraceInformation(string.Format("TIME--{0}::THREAD--{1}/{2}::MSG--{3}",
                                                                    DateTime.Now.TimeOfDay,
                                                                    Thread.CurrentThread.ManagedThreadId, Process.GetCurrentProcess().Id, msg));
        }
    }
}
