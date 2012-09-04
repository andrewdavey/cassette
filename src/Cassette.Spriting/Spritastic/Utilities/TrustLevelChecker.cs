using System;
using System.Web;

namespace Cassette.Spriting.Spritastic.Utilities
{
    static class TrustLevelChecker
    {
        // Based on 
        // http://blogs.msdn.com/b/dmitryr/archive/2007/01/23/finding-out-the-current-trust-level-in-asp-net.aspx
        public static AspNetHostingPermissionLevel GetCurrentTrustLevel()
        {
            foreach (var trustLevel in
                    new[] {
                    AspNetHostingPermissionLevel.Unrestricted,
                    AspNetHostingPermissionLevel.High,
                    AspNetHostingPermissionLevel.Medium,
                    AspNetHostingPermissionLevel.Low,
                    AspNetHostingPermissionLevel.Minimal 
                })
            {
                try
                {
                    new AspNetHostingPermission(trustLevel).Demand();
                }
                catch (Exception)
                {
                    continue;
                }

                return trustLevel;
            }

            return AspNetHostingPermissionLevel.None;
        }

        public static bool IsFullTrust()
        {
            return GetCurrentTrustLevel() == AspNetHostingPermissionLevel.Unrestricted;
        }
    }
}
