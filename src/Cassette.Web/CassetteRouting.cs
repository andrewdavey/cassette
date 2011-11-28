#region License
/*
Copyright 2011 Andrew Davey

This file is part of Cassette.

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
*/
#endregion

using System;
using System.Web.Routing;
using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Cassette.Utilities;

namespace Cassette.Web
{
    class CassetteRouting : IUrlGenerator
    {
        readonly IUrlModifier urlModifier;
        const string RoutePrefix = "_cassette";

        public CassetteRouting(IUrlModifier urlModifier)
        {
            this.urlModifier = urlModifier;
        }

        public void InstallRoutes(RouteCollection routes, IBundleContainer bundleContainer)
        {
            using (routes.GetWriteLock())
            {
                RemoveExistingCassetteRoutes(routes);

                InstallBundleRoute<ScriptBundle>(routes, bundleContainer);
                InstallBundleRoute<StylesheetBundle>(routes, bundleContainer);
                InstallBundleRoute<HtmlTemplateBundle>(routes, bundleContainer);
                InstallHudRoute(routes, bundleContainer);

                InstallRawFileRoute(routes);

                InstallAssetCompileRoute(routes, bundleContainer);
            }
        }

        public string CreateBundleUrl(Bundle bundle)
        {
            return urlModifier.Modify(string.Format("{0}/{1}/{2}_{3}",
                RoutePrefix,
                ConventionalBundlePathName(bundle.GetType()),
                bundle.Path.Substring(2),
                bundle.Hash.ToHexString()
            ));
        }

        public string CreateAssetUrl(IAsset asset)
        {
            return urlModifier.Modify(string.Format(
                "{0}/asset/{1}?{2}",
                RoutePrefix,
                asset.SourceFile.FullPath.Substring(2),
                asset.Hash.ToHexString()
            ));
        }

        public string CreateRawFileUrl(string filename, string hash)
        {
            if (filename.StartsWith("~") == false)
            {
                throw new ArgumentException("Image filename must be application relative (starting with '~').");
            }

            filename = filename.Substring(2); // Remove the "~/"
            var dotIndex = filename.LastIndexOf('.');
            var name = filename.Substring(0, dotIndex);
            var extension = filename.Substring(dotIndex + 1);
            
            return urlModifier.Modify(string.Format("{0}/file/{1}_{2}_{3}",
                RoutePrefix,
                ConvertToForwardSlashes(name),
                hash,
                extension
            ));
        }

        static void RemoveExistingCassetteRoutes(RouteCollection routes)
        {
            for (int i = routes.Count - 1; i >= 0; i--)
            {
                if (routes[i] is CassetteRoute)
                {
                    routes.RemoveAt(i);
                }
            }
        }

        void InstallBundleRoute<T>(RouteCollection routes, IBundleContainer bundleContainer)
            where T : Bundle
        {
            var url = GetBundleRouteUrl<T>();
            var handler = new DelegateRouteHandler(
                requestContext => new BundleRequestHandler<T>(bundleContainer, requestContext)
            );
            Trace.Source.TraceInformation("Installing {0} route handler for \"{1}\".", typeof(T).FullName, url);
            // Insert Cassette's routes at the start of the table, 
            // to avoid conflicts with the application's own routes.
            routes.Insert(0, new CassetteRoute(url, handler));
        }

        string GetBundleRouteUrl<T>()
        {
            return string.Format(
                "{0}/{1}/{{*path}}",
                RoutePrefix,
                ConventionalBundlePathName(typeof(T))
            );
        }

        void InstallHudRoute(RouteCollection routes, IBundleContainer bundleContainer)
        {
            routes.Insert(0, new CassetteRoute(RoutePrefix, new DelegateRouteHandler(context => new HudRequestHandler(bundleContainer, context, this))));
        }

        void InstallRawFileRoute(RouteCollection routes)
        {
            const string url = RoutePrefix + "/file/{*path}";
            var handler = new DelegateRouteHandler(
                requestContext => new RawFileRequestHandler(requestContext)
            );
            Trace.Source.TraceInformation("Installing raw file route handler for \"{0}\".", url);
            // Insert Cassette's routes at the start of the table, 
            // to avoid conflicts with the application's own routes.
            routes.Insert(0, new CassetteRoute(url, handler));
        }

        void InstallAssetCompileRoute(RouteCollection routes, IBundleContainer bundleContainer)
        {
            // Used to return compiled coffeescript, less, etc.
            const string url = RoutePrefix + "/asset/{*path}";
            var handler = new DelegateRouteHandler(
                requestContext => new AssetRequestHandler(
                    requestContext,
                    bundleContainer
                )
            );
            Trace.Source.TraceInformation("Installing asset route handler for \"{0}\".", url);
            // Insert Cassette's routes at the start of the table, 
            // to avoid conflicts with the application's own routes.
            routes.Insert(0, new CassetteRoute(url, handler));
        }

        static string ConventionalBundlePathName(Type bundleType)
        {
            // ExternalScriptBundle subclasses ScriptBundle, but we want the name to still be "scripts"
            // So walk up the inheritance chain until we get to something that directly inherits from Bundle.
            while (bundleType != null && bundleType.BaseType != typeof(Bundle))
            {
                bundleType = bundleType.BaseType;
            }
            if (bundleType == null) throw new ArgumentException("Type must be a subclass of Cassette.Bundle.", "bundleType");

            return bundleType.Name.ToLowerInvariant();
        }

        string ConvertToForwardSlashes(string path)
        {
            return path.Replace('\\', '/');
        }
    }
}

