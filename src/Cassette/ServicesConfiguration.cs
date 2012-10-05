using System;
using System.Collections.Generic;
using Cassette.BundleProcessing;
using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Cassette.TinyIoC;

namespace Cassette
{
    /// <summary>
    /// Convenience base class for overriding Cassette's default services e.g. <see cref="IUrlModifier"/>
    /// </summary>
    public abstract class ServicesConfiguration : IConfiguration<TinyIoCContainer>
    {
        Type urlModifierType;
        Type javaScriptMinifierType;
        Type stylesheetMinifierType;
        Type jsonSerializerType;
        readonly Dictionary<Type, IFileSearch> fileSearches = new Dictionary<Type, IFileSearch>();
        readonly List<Action<TinyIoCContainer>> configurations = new List<Action<TinyIoCContainer>>();

        public virtual void Configure(TinyIoCContainer container)
        {
            if (UrlModifierType != null)
            {
                container.Register(typeof(IUrlModifier), UrlModifierType);
            }
            if (JavaScriptMinifierType != null)
            {
                container.Register(typeof(IJavaScriptMinifier), JavaScriptMinifierType);
            }
            if (StylesheetMinifierType != null)
            {
                container.Register(typeof(IStylesheetMinifier), StylesheetMinifierType);
            }
            if (JsonSerializerType != null)
            {
                container.Register(typeof(IJsonSerializer), JsonSerializerType);
            }
            RegisterFileSearches(container);
            configurations.ForEach(c => c(container));
        }

        void RegisterFileSearches(TinyIoCContainer container)
        {
            foreach (var fileSearch in fileSearches)
            {
                var bundleType = fileSearch.Key;
                container.Register(
                    typeof(IFileSearch),
                    fileSearch.Value,
                    HostBase.FileSearchComponentName(bundleType)
                );
            }
        }

        /// <summary>
        /// The type that implements <see cref="IUrlModifier"/>.
        /// </summary>
        public Type UrlModifierType
        {
            get { return urlModifierType; }
            set
            {
                if (value != null && !typeof(IUrlModifier).IsAssignableFrom(value))
                    throw new ArgumentException(string.Format("UrlModifierType {0} must implement {1}", value.FullName, typeof(IUrlModifier).FullName));
                urlModifierType = value;
            }
        }

        /// <summary>
        /// The type that implements <see cref="IJavaScriptMinifier"/>.
        /// </summary>
        public Type JavaScriptMinifierType
        {
            get { return javaScriptMinifierType; }
            set
            {
                if (value != null && !typeof(IJavaScriptMinifier).IsAssignableFrom(value))
                    throw new ArgumentException(string.Format("JavaScriptMinifierType {0} must implement {1}", value.FullName, typeof(IJavaScriptMinifier).FullName));
                javaScriptMinifierType = value;
            }
        }

        /// <summary>
        /// The type that implements <see cref="IStylesheetMinifier"/>.
        /// </summary>
        public Type StylesheetMinifierType
        {
            get
            {
                return stylesheetMinifierType;
            }
            set
            {
                if (value != null && !typeof(IStylesheetMinifier).IsAssignableFrom(value))
                    throw new ArgumentException(string.Format("StylesheetMinifierType {0} must implement {1}", value.FullName, typeof(IStylesheetMinifier).FullName));
                stylesheetMinifierType = value;
            }
        }

        public Type JsonSerializerType
        {
            get { return jsonSerializerType; }
            set
            {
                if (value != null && !typeof(IJsonSerializer).IsAssignableFrom(value))
                    throw new ArgumentException(string.Format("JsonSerializerType {0} must implement {1}", value.FullName, typeof(IJsonSerializer).FullName));
                jsonSerializerType = value;
            }
        }

        public void SetDefaultFileSearch<T>(IFileSearch fileSearch)
            where T: Bundle
        {
            fileSearches[typeof(T)] = fileSearch;
        }

        public JavaScriptHtmlTemplateConfiguration ConvertHtmlTemplatesIntoJavaScript()
        {
            AddConfiguration(container =>
            {
                container.Register<IHtmlTemplateScriptStrategy, DomHtmlTemplateScriptStrategy>();
                container.Register<IBundlePipeline<HtmlTemplateBundle>>(
                    (c, n) => new JavaScriptHtmlTemplatePipeline(
                        c,
                        c.Resolve<CassetteSettings>(),
                        c.Resolve<IJavaScriptMinifier>(),
                        c.Resolve<RemoteHtmlTemplateBundleRenderer>()
                    )
                );
            });
            return new JavaScriptHtmlTemplateConfiguration(AddConfiguration);
        }

        public class JavaScriptHtmlTemplateConfiguration
        {
            readonly Action<Action<TinyIoCContainer>> addConfiguration;

            public JavaScriptHtmlTemplateConfiguration(Action<Action<TinyIoCContainer>> addConfiguration)
            {
                this.addConfiguration = addConfiguration;
            }

            public JavaScriptHtmlTemplateConfiguration StoreInGlobalVariable(string globalVarName = "JST")
            {
                addConfiguration(container => container.Register<IHtmlTemplateScriptStrategy>(
                    (c, n) => new GlobalVarHtmlTemplateScriptStrategy(
                        globalVarName,
                        c.Resolve<IJsonSerializer>()
                    )
                ));
                return this;                
            }

            public JavaScriptHtmlTemplateConfiguration UsingScriptStrategy<THtmlTemplateScriptStrategy>()
                where THtmlTemplateScriptStrategy : class, IHtmlTemplateScriptStrategy
            {
                addConfiguration(
                    container => container.Register<IHtmlTemplateScriptStrategy, THtmlTemplateScriptStrategy>()
                );
                return this;
            }

            public JavaScriptHtmlTemplateConfiguration UsingScriptStrategy(IHtmlTemplateScriptStrategy scriptStrategy)
            {
                addConfiguration(
                    container => container.Register(scriptStrategy)
                );
                return this;
            }

            public JavaScriptHtmlTemplateConfiguration UsingIdStrategy<THtmlTemplateIdStrategy>()
                where THtmlTemplateIdStrategy : class, IHtmlTemplateIdStrategy
            {
                addConfiguration(
                    container => container.Register<IHtmlTemplateIdStrategy, THtmlTemplateIdStrategy>()
                );
                return this;
            }

            public JavaScriptHtmlTemplateConfiguration UsingIdStrategy(IHtmlTemplateIdStrategy idStrategy)
            {
                addConfiguration(
                    container => container.Register(idStrategy)
                );
                return this;
            }
        }

        void AddConfiguration(Action<TinyIoCContainer> configuration)
        {
            configurations.Add(configuration);
        }
    }
}