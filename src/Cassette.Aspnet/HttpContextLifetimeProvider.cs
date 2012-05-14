using System;
using System.Web;
using Cassette.TinyIoC;

namespace Cassette.Aspnet
{
    class HttpContextLifetimeProvider : RequestLifetimeProviderBase
    {
        readonly Func<HttpContextBase> getHttpContext;
        readonly string keyName = String.Format("TinyIoC.HttpContext.{0}", Guid.NewGuid());
        bool isSet;

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
            isSet = true;
            getHttpContext().Items[keyName] = value;
        }

        public override void ReleaseObject()
        {
            if (!isSet) return;
            base.ReleaseObject();
        }
    }
}