using System;

namespace TinyIoC
{
    public static class TinyIoCExtensions
    {
        public static TinyIoCContainer.RegisterOptions AsPerRequestSingleton(this TinyIoCContainer.RegisterOptions registerOptions, TinyIoCContainer.ITinyIoCObjectLifetimeProvider lifetimeProvider)
        {
            return TinyIoCContainer.RegisterOptions.ToCustomLifetimeManager(
                registerOptions, 
                lifetimeProvider,
                "per request singleton"
            );
        }
    }

    public abstract class RequestLifetimeProviderBase : TinyIoCContainer.ITinyIoCObjectLifetimeProvider
    {
        public abstract object GetObject();

        public abstract void SetObject(object value);

        public virtual void ReleaseObject()
        {
            var item = GetObject() as IDisposable;
            if (item != null)
            {
                item.Dispose();
            }
            SetObject(null);
        }
    }
}