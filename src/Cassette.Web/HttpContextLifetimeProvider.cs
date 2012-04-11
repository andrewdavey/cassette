using System;
using System.Web;
using TinyIoC;

namespace Cassette.Web
{
    class HttpContextLifetimeProvider : RequestLifetimeProviderBase
    {
        readonly Func<HttpContextBase> getHttpContext;
        readonly string keyName = String.Format("TinyIoC.HttpContext.{0}", Guid.NewGuid());

        public HttpContextLifetimeProvider(Func<HttpContextBase> getHttpContext)
        {
            this.getHttpContext = getHttpContext;
        }

        public override object GetObject()
        {
            return getHttpContext().Items[keyName];
        }

        public override void SetObject(object value)
        {
            getHttpContext().Items[keyName] = value;
        }
    }
}