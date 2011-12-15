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

using Cassette.Configuration;
using Cassette.IO;
using IsolatedStorageFile = System.IO.IsolatedStorage.IsolatedStorageFile;

namespace Cassette.Web
{
    class InitialConfiguration : ICassetteConfiguration
    {
        readonly CassetteConfigurationSection configurationSection;
        readonly bool globalIsDebuggingEnabled;
        readonly string sourceDirectory;
        readonly string virtualDirectory;
        readonly IsolatedStorageFile storage;

        public InitialConfiguration(CassetteConfigurationSection configurationSection, bool globalIsDebuggingEnabled, string sourceDirectory, string virtualDirectory, IsolatedStorageFile storage)
        {
            this.configurationSection = configurationSection;
            this.globalIsDebuggingEnabled = globalIsDebuggingEnabled;
            this.sourceDirectory = sourceDirectory;
            this.virtualDirectory = virtualDirectory;
            this.storage = storage;
        }

        public void Configure(BundleCollection bundles, CassetteSettings settings)
        {
            settings.IsDebuggingEnabled = 
                configurationSection.Debug.HasValue && configurationSection.Debug.Value ||
               (!configurationSection.Debug.HasValue && globalIsDebuggingEnabled);

            settings.IsHtmlRewritingEnabled = configurationSection.RewriteHtml;

            settings.SourceDirectory = new FileSystemDirectory(sourceDirectory);
            settings.CacheDirectory = new IsolatedStorageDirectory(storage);
            settings.UrlModifier = new VirtualDirectoryPrepender(virtualDirectory);
        }
    }
}
