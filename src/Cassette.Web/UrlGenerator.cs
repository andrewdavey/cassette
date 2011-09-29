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
using Cassette.Utilities;

namespace Cassette.Web
{
    public class UrlGenerator : IUrlGenerator
    {
        public UrlGenerator(string virtualDirectory, string assetUrlPrefix = "_assets")
        {
            this.virtualDirectory = virtualDirectory.TrimEnd('/');
            this.assetUrlPrefix = assetUrlPrefix;
        }

        readonly string virtualDirectory;
        readonly string assetUrlPrefix;

        public string GetModuleRouteUrl<T>()
        {
            return string.Format(
                "{0}/{1}/{{*path}}",
                assetUrlPrefix,
                ConventionalModulePathName(typeof(T))
            );
        }

        public string GetAssetRouteUrl()
        {
            return assetUrlPrefix + "/get/{*path}";
        }

        public string GetRawFileRouteUrl()
        {
            return assetUrlPrefix + "/file/{*path}";
        }

        public string CreateModuleUrl(Module module)
        {
            return string.Format("{0}/{1}/{2}/{3}_{4}",
                virtualDirectory,
                assetUrlPrefix,
                ConventionalModulePathName(module.GetType()),
                module.Path.Substring(2),
                module.Assets[0].Hash.ToHexString()
            );
        }

        public string CreateAssetUrl(IAsset asset)
        {
            return string.Format(
                "{0}/{1}?{2}",
                virtualDirectory,
                asset.SourceFilename.Substring(2),
                asset.Hash.ToHexString()
            );
        }

        public string CreateAssetCompileUrl(Module module, IAsset asset)
        {
            return string.Format(
                "{0}/{1}/get/{2}?{3}",
                virtualDirectory,
                assetUrlPrefix,
                asset.SourceFilename.Substring(2),
                asset.Hash.ToHexString()
            );
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

            return string.Format("{0}/{1}/images/{2}_{3}_{4}",
                virtualDirectory,
                assetUrlPrefix,
                ConvertToForwardSlashes(name),
                hash,
                extension
            );
        }

        string ConventionalModulePathName(Type moduleType)
        {
            // ExternalScriptModule subclasses ScriptModule, but we want the name to still be "scripts"
            // So walk up the inheritance chain until we get to something that directly inherits from Module.
            while (moduleType.BaseType != typeof(Module))
            {
                moduleType = moduleType.BaseType;
            }

            var name = moduleType.Name;
            name = name.Substring(0, name.Length - "Module".Length);
            return name.ToLowerInvariant() + "s";
        }

        string ConvertToForwardSlashes(string path)
        {
            return path.Replace('\\', '/');
        }
    }
}
