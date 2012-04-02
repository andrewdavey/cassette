using System;
using System.Web;

namespace Cassette.Web
{
    /// <summary>
    /// Adds an <see cref="IReferenceBuilder"/> to the HttpContext Items collection for each HTTP request.
    /// </summary>
    public class ReferenceBuilderInstaller
    {
        readonly Func<HttpContextBase> getHttpContext;
        readonly Func<IReferenceBuilder> createReferenceBuilder;

        static readonly string ReferenceBuilderKey = typeof(IReferenceBuilder).FullName;

        public ReferenceBuilderInstaller(Func<HttpContextBase> getHttpContext, Func<IReferenceBuilder> createReferenceBuilder)
        {
            this.getHttpContext = getHttpContext;
            this.createReferenceBuilder = createReferenceBuilder;
        }

        public void Install(HttpApplication httpApplication)
        {
            httpApplication.PostMapRequestHandler += HttpApplicationOnPostMapRequestHandler;
        }

        void HttpApplicationOnPostMapRequestHandler(object sender, EventArgs eventArgs)
        {
            AddReferenceBuilderToHttpContextItems();
        }

        void AddReferenceBuilderToHttpContextItems()
        {
            var context = getHttpContext();
            context.Items[ReferenceBuilderKey] = createReferenceBuilder();
        }
    }
}