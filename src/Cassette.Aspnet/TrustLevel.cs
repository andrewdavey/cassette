using System.Security;
using System.Web;

namespace Cassette.Aspnet
{
    class TrustLevel
    {
        public static AspNetHostingPermissionLevel GetCurrentTrustLevel()
        {
            var trustLevels = new[]
            {
                AspNetHostingPermissionLevel.Unrestricted,
                AspNetHostingPermissionLevel.High,
                AspNetHostingPermissionLevel.Medium,
                AspNetHostingPermissionLevel.Low,
                AspNetHostingPermissionLevel.Minimal
            };
            foreach (var trustLevel in trustLevels)
            {
                try
                {
                    new AspNetHostingPermission(trustLevel).Demand();
                }
                catch (SecurityException)
                {
                    continue;
                }

                return trustLevel;
            }

            return AspNetHostingPermissionLevel.None;
        }

        public static bool IsFullTrust()
        {
            var trustLevel = GetCurrentTrustLevel();
            return trustLevel == AspNetHostingPermissionLevel.High ||
                   trustLevel == AspNetHostingPermissionLevel.Unrestricted;
        }
    }
}