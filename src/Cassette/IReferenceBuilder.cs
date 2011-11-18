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


namespace Cassette
{
    public interface IReferenceBuilder
    {
        /// <summary>
        /// Adds a reference to the asset bundle with the given path.
        /// </summary>
        /// <param name="path">The application relative path to the asset bundle.</param>
        /// <param name="location">Optional page render location for the asset bundle.</param>
        void Reference(string path, string location = null);

        /// <summary>
        /// Adds a reference to the asset bundle.
        /// </summary>
        /// <param name="bundle">The asset bundle.</param>
        /// <param name="location">Optional render location for the asset bundle.</param>
        void Reference(Bundle bundle, string location = null);

        /// <summary>
        /// Returns the HTML elements that include into the page all the referenced bundles and their dependencies.
        /// </summary>
        /// <param name="location">Optional. The page location that is being rendered.</param>
        /// <returns>The HTML elements that include into the page all the referenced bundles and their dependencies</returns>
        string Render<T>(string location) where T : Bundle;
    }
}