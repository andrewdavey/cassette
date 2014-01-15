using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Compilation;
using Cassette.TinyIoC;

namespace Cassette.Aspnet
{
    public class WebHost : HostBase
    {
        protected override IEnumerable<Assembly> LoadAssemblies()
        {
            return BuildManager.GetReferencedAssemblies().Cast<Assembly>();
        }

        protected override bool CanCreateRequestLifetimeProvider
        {
            get { return true; }
        }

        protected override TinyIoCContainer.ITinyIoCObjectLifetimeProvider CreateRequestLifetimeProvider()
        {
            return new HttpContextLifetimeProvider(() => Container.Resolve<HttpContextBase>());
        }

        protected override void ConfigureContainer()
        {
            // These are before base.ConfigureContainer() so the application is able to override them - for example providing a different IUrlModifier.
            Container.Register((c, p) => HttpContext());
            Container.Register((c, p) => c.Resolve<HttpContextBase>().Request);
            Container.Register((c, p) => c.Resolve<HttpContextBase>().Response);
            Container.Register(typeof(IUrlModifier), CreateUrlModifier());
            
            Container.Register<ICassetteRequestHandler, AssetRequestHandler>("AssetRequestHandler").AsPerRequestSingleton(CreateRequestLifetimeProvider());
            Container.Register<ICassetteRequestHandler>(
                (c, n) =>
                new CachedFileRequestHandler(c.Resolve<HttpRequestBase>(), c.Resolve<HttpResponseBase>(), c.Resolve<CassetteSettings>().CacheDirectory),
                "CachedFileRequestHandler"
            );
            Container.Register<ICassetteRequestHandler, BundleRequestHandler<Scripts.ScriptBundle>>("ScriptBundleRequestHandler").AsPerRequestSingleton(CreateRequestLifetimeProvider());
            Container.Register<ICassetteRequestHandler, BundleRequestHandler<Stylesheets.StylesheetBundle>>("StylesheetBundleRequestHandler").AsPerRequestSingleton(CreateRequestLifetimeProvider());
            Container.Register<ICassetteRequestHandler, BundleRequestHandler<HtmlTemplates.HtmlTemplateBundle>>("HtmlTemplateBundleRequestHandler").AsPerRequestSingleton(CreateRequestLifetimeProvider());
            Container.Register<IDiagnosticRequestHandler, DiagnosticRequestHandler>().AsPerRequestSingleton(CreateRequestLifetimeProvider());
            Container.Register<RawFileRequestRewriter>().AsPerRequestSingleton(CreateRequestLifetimeProvider());

            base.ConfigureContainer();
        }

        IUrlModifier CreateUrlModifier()
        {
            return new VirtualDirectoryPrepender(AppDomainAppVirtualPath);
        }

        protected virtual string AppDomainAppVirtualPath
        {
            get { return HttpRuntime.AppDomainAppVirtualPath; }
        }

        protected virtual HttpContextBase HttpContext()
        {
            return new HttpContextWrapper(System.Web.HttpContext.Current);
        }

        public PlaceholderRewriter CreatePlaceholderRewriter()
        {
            return Container.Resolve<PlaceholderRewriter>();
        }

        protected override IConfiguration<CassetteSettings> CreateHostSpecificSettingsConfiguration()
        {
            return new WebHostSettingsConfiguration(AppDomainAppVirtualPath);
        }

        const string RequestContainerKey = "CassetteRequestContainer";

        public void StoreRequestContainerInHttpContextItems()
        {
            var context = HttpContext();
            context.Items[RequestContainerKey] = Container.GetChildContainer();
        }

        public TinyIoCContainer RequestContainer
        {
            get { return HttpContext().Items[RequestContainerKey] as TinyIoCContainer; }
        }

        public void RemoveRequestContainerFromHttpContextItems()
        {
            var context = HttpContext();
            var container = context.Items[RequestContainerKey] as TinyIoCContainer;
            if (container != null)
            {
                container.Dispose();
                context.Items.Remove(RequestContainerKey);
            }
        }
    }
}