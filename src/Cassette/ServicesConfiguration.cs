using System;
using System.Collections.Generic;
using TinyIoC;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Cassette.Configuration;

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
        readonly Dictionary<Type, IFileSearch> fileSearches = new Dictionary<Type, IFileSearch>();

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
            RegisterFileSearches(container);
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

        public void SetDefaultFileSearch<T>(IFileSearch fileSearch)
            where T: Bundle
        {
            fileSearches[typeof(T)] = fileSearch;
        }
    }
}