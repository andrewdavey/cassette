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

namespace Cassette.Configuration
{
    /// <summary>
    /// The local assets settings for an external bundle.
    /// </summary>
    public class LocalAssetSettings
    {
        public LocalAssetSettings()
        {
            Path = "~/";
            UseWhenDebugging = true;
        }

        /// <summary>
        /// Gets or sets the application relative path to the local assets.
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// Gets or sets if the local assets should be used when application is in debug mode. Default is true.
        /// </summary>
        public bool UseWhenDebugging { get; set; }
        /// <summary>
        /// Gets or sets the <see cref="IFileSearch"/> used to find asset files. If null the bundle type's application default <see cref="IFileSearch"/> will be used.
        /// </summary>
        public IFileSearch FileSearch { get; set; }
        /// <summary>
        /// Gets or sets a JavaScript fallback condition. Used to load the local assets when the remote asset has failed to load.
        /// </summary>
        public string FallbackCondition { get; set; }
    }
}
