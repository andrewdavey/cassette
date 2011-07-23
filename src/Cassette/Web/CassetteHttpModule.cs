using System;
using System.Web;
using System.Web.Caching;

namespace Cassette.Web
{
    public class CassetteHttpModule : IHttpModule
    {
        public void Init(HttpApplication application)
        {
            application.BeginRequest += (sender, e) =>
            {
                var httpContext = new HttpContextWrapper(HttpContext.Current);
                var pageHelper = SingleManagerContainer.Manager.CreatePageHelper(httpContext);
                StorePageHelperInHttpContextItems(pageHelper, httpContext);
            };
        }

        void StorePageHelperInHttpContextItems(IPageHelper pageHelper, HttpContextBase httpContext)
        {
            httpContext.Items["Cassette.PageHelper"] = pageHelper;
        }

        public static IPageHelper GetPageHelper(HttpContextBase httpContext = null)
        {
            if (httpContext == null) httpContext = new HttpContextWrapper(HttpContext.Current);

            var helper = httpContext.Items["Cassette.PageHelper"] as IPageHelper;
            if (helper == null)
            {
                throw new InvalidOperationException("Cassette.PageHelper has not been added to the current HttpContext Items. Make sure the CassetteHttpModule has been added to Web.config.");
            }
            return helper;
        }

        public void Dispose()
        {
        }
    }
}
